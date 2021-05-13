using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CommandSystem;
using EventfulLaboratory.Extension;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Permissions.Extensions;
using MEC;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

namespace EventfulLaboratory.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    internal sealed class BuilderCmd : ICommand
    {
        private static BuilderOptions _builderOptions = new BuilderOptions();
        
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = null; //I hate how out works
            
            var args = arguments.Array;
            if (args == null)
            {
                response = "Args is null, something went wrong O.o";
                return false;
            }
            
            
            var usr = sender as CommandSender;
            if (!usr.CheckPermission("builder.build"))
            {
                response = "No permission!";
                return false;
            }
            
            var player = Player.Get(usr);

            if (_builderOptions.IsSetup() && !_builderOptions.IsBuilder(player))
            {
                response = "Builder is setup for another player!";
                return false;
            }

            if (!_builderOptions.IsSetup() && (args.Length < 2 || args[1] != "init"))
            {
                response = "Builder is not setup yet! Use init first!";
                return false;
            }
            
            try
            {
                switch (args[1])
                {
                    case "init":
                        _builderOptions.Init(player);
                        break;
                    case "args":
                        response = args.Aggregate("", (current, s) => current + (s + ", "));
                        return true;
                    case "ray":
                        var ray = player.Raytrace();
                        player.RemoteAdminMessage($"Ray:{ray}\nPos:{player.Position}\nCollider: {ray.collider}\nDistance: {ray.distance}\nTransPos: {ray.transform}\nRotation{player.Rotation}\nAt: {ray.point}");
                        break;
                    default:
                        if (_builderOptions.HandleArgs(args))
                        {
                            response = $"Complete.";
                            return true;
                        }
                        else
                        {
                            response = $"Unknown builder command: {args[1]}";
                            return false;
                        }
                }
            }
            catch (Exception e)
            {
                response = $"Builder cmd failed: {e.Message}";
                return false;
            }

            if (response == null)
                response = "Builder done";
            return true;
        }

        public string Command { get; } = "builder";
        public string[] Aliases { get; } = new []{ "bld"};
        public string Description { get; } = "builder";
    }

    internal sealed class BuilderOptions
    {
        public Player BuilderPlayer; //Bob
        public GameObject TargetObject { get; private set; }

        private List<GameObject> _builtPlatformKeeper = new List<GameObject>();
        private Vector3 _center;

        private float _mult = 1f;

        public void Init(Player player)
        {
            Exiled.Events.Handlers.Player.Shooting += ShootingEvent;
            //Exiled.Events.Handlers.Player.SyncingData += MovementEvent;
            BuilderPlayer = player;
            _center = player.Position;
            Timing.RunCoroutine(SetupBuilder(player));
        }

        ~BuilderOptions()
        {
            Exiled.Events.Handlers.Player.Shooting -= ShootingEvent;
            //Exiled.Events.Handlers.Player.SyncingData -= MovementEvent;
        }

        private void Hint(string hint, float duration = 3f) => BuilderPlayer.ShowHint(hint, duration);
        private void RA(string str) => BuilderPlayer.RemoteAdminMessage(str);
        
        public bool IsSetup() => BuilderPlayer != null;

        public bool IsBuilder(Player player) => player != null && IsBuilder(player.UserId);

        private bool IsBuilder(string userId) => BuilderPlayer.UserId == userId;

        private void ShootingEvent(ShootingEventArgs ev)
        {
            Logger.Info("ShotEvent");
            //If not our Builder
            if (ev.Shooter == null || !IsBuilder(ev.Shooter)) return;

            try
            {
                switch (ev.Shooter.Inventory.curItem)
                {
                    case ItemType.GunUSP: {
                        //Case Scoping, cause fuck variable init shadow amiright
                        var ray = ev.Shooter.Raytrace();
                        if (!ray.IsHit())
                        {
                            Hint($"Miss.\nAt: {ray.transform.position}\nPos: {ev.Shooter.Position}");
                        }
                        else
                        {
                            var thingie = ray.collider.gameObject.FindNetworkIdentityParentObj();
                            if (thingie != null)
                            {
                                Hint($"Collision Detected: {thingie.name}");
                                TargetObject = thingie;
                            }
                            else
                            {
                                Hint("Miss or not Movable.");
                            }
                            
                        }
                    }break;
                    case ItemType.GunProject90: {
                        var ray = ev.Shooter.Raytrace();
                        if (ray.IsHit())
                        {
                            Common.JustFuckingSpawnADoor(ray.point + new Vector3(0, 0.5f, 0f));
                            Hint($"Door spawned at shot location\n{ray.point}\n{ev.Shooter.Position}");
                        }
                        else
                        {
                            Hint("Miss.");
                        }
                        
                    }break;
                    case ItemType.GunCOM15: {
                        if (TargetObject != null)
                        {
                            var ray = ev.Shooter.Raytrace();
                            if (ray.IsHit())
                            {
                                TargetObject.transform.position = ray.point;
                                TargetObject.NotifyPlayers();
                                Hint($"Object moved to {ray.point}.");
                            }
                            else
                            {
                                Hint("Miss.");
                            }

                        }
                        else
                        {
                            Hint($"No selected object.");
                        }
                    }break;
                }
            }
            catch (Exception e)
            {
                ev.Shooter.ShowHint($"ShootEvent failed: {e.Message}");
                Logger.Error(e.StackTrace);
            }
        }

        private static int _syncThrottle = 0;

        private void MovementEvent(SyncingDataEventArgs ev)
        {
            if (!IsSetup()) return;
            if (ev.Player.UserId != BuilderPlayer.UserId) return;

            if (ev.Player.Inventory.curItem != ItemType.KeycardO5) return; //Not O5, The Editor
            if (TargetObject == null) return;
            
            ev.Player.Position = ev.Player.ReferenceHub.playerMovementSync.LastSafePosition;
            var oldVec = ev.Player.ReferenceHub.playerMovementSync.PlayerVelocity.Copy();
            ev.Player.ReferenceHub.playerMovementSync.PlayerVelocity = Vector3.zero;
            
            _syncThrottle++;
            if (_syncThrottle < 10) return;
            _syncThrottle = 0;
            

            var velo = oldVec.ClampToOne(3f);
            switch (ev.Player.MoveState)
            {
                case PlayerMovementState.Sneaking: {
                    TargetObject.transform.rotation = Quaternion.Euler((velo * _mult) + TargetObject.transform.rotation.eulerAngles);
                }break;
                
                case PlayerMovementState.Walking: {
                    TargetObject.transform.position += velo * _mult;
                }break;
                
                case PlayerMovementState.Sprinting: {
                    TargetObject.transform.localScale += velo * _mult;
                }break;
            }
            
            Hint($"{TargetObject.name}\nPos: {TargetObject.transform.position}\nRot: {TargetObject.transform.rotation.eulerAngles}\nScl:{TargetObject.transform.localScale}\nVelo: {ev.Player.ReferenceHub.playerMovementSync.PlayerVelocity}\n{ev.Player.ReferenceHub.playerMovementSync.PlayerVelocity.ClampToOne(3f)}");

            if (velo.AnyNonZero())
            {
                TargetObject.NotifyPlayers();
            }
        }
        
        #region BuilderPlayer Setup 
        private static readonly ItemType[] _weapons =
        {
            ItemType.GunUSP,
            ItemType.GunProject90,
            ItemType.GunCOM15,
            ItemType.GunLogicer,
            ItemType.GunMP7,
            ItemType.GunE11SR,
            ItemType.KeycardO5
        };

        public static IEnumerator<float> SetupBuilder(Player player)
        {
            player.SetRole(RoleType.Tutorial);
            yield return Timing.WaitForSeconds(0.5f);
            player.NoClipEnabled = true;
            player.IsGodModeEnabled = true;
            player.Ammo[(int) AmmoType.Nato9] = 3000;
            player.Ammo[(int) AmmoType.Nato556] = 3000;
            player.Ammo[(int) AmmoType.Nato762] = 3000;
            
            foreach (var itemType in _weapons)
            {
                player.AddItem(new Inventory.SyncItemInfo
                {
                    id = itemType,
                    durability = 200f,
                });
            }
        }
        #endregion
        
        #region PosRotScl Handlers

        public void HandlePos(string[] args)
        {
            var subArgs = args.Skip(2).ToList();
            Logger.Info(subArgs.ToString());
            Logger.Info(subArgs[0]);
            try
            {
                switch (subArgs.Count)
                {
                    case 0: {
                        RA($"Pos: {TargetObject.transform.position}");
                    }break;
                    case 1:
                    {
                        var scale = float.Parse(subArgs[0]);
                        var prev = TargetObject.transform.position.ToString();
                        var toAdd = BuilderPlayer.Rotation * scale;
                        TargetObject.transform.position += toAdd;
                        RA($"Moved object {TargetObject.name}\nOldPos: {prev}\nNewPos: {TargetObject.transform.position}");
                        TargetObject.NotifyPlayers();
                    }break;
                    case 2:
                    {
                        var (x1,y1) = (float.Parse(subArgs[0]), float.Parse(subArgs[1]));
                        var prev = TargetObject.transform.position.ToString();
                        var toAdd = new Vector3(x1, y1);
                        TargetObject.transform.position += toAdd;
                        RA($"Moved object {TargetObject.name}\nOldPos: {prev}\nNewPos: {TargetObject.transform.position}");
                        TargetObject.NotifyPlayers();
                    }break;
                    case 3:
                    {
                        var (x1,y1,z1) = (float.Parse(subArgs[0]), float.Parse(subArgs[1]), float.Parse(subArgs[2]));
                        var prev = TargetObject.transform.position.ToString();
                        var toAdd = new Vector3(x1,y1,z1);
                        TargetObject.transform.position += toAdd;
                        RA($"Moved object {TargetObject.name}\nOldPos: {prev}\nNewPos: {TargetObject.transform.position}");
                        TargetObject.NotifyPlayers();
                    }break;
                }
            }
            catch (Exception e)
            {
                RA($"Failed:{e.Message}\n{e.StackTrace}");
            }
        }

        public void HandleRot(string[] args)
        {
            var subArgs = args.Skip(2).ToList();
            try
            {
                switch (subArgs.Count)
                {
                    case 0:
                    {
                        RA($"Rot: {TargetObject.transform.rotation.eulerAngles}");
                    }
                        break;
                    case 1:
                    {
                        var scale = float.Parse(subArgs[0]);
                        var prev = TargetObject.transform.rotation.eulerAngles;
                        var toAdd = BuilderPlayer.Rotation * scale;
                        TargetObject.transform.rotation = Quaternion.Euler(prev + toAdd);
                        RA($"Rotated object {TargetObject.name}\nOldRot: {prev}\nNewRot: {TargetObject.transform.rotation.eulerAngles}");
                        TargetObject.NotifyPlayers();
                    }
                        break;
                    case 2:
                        RA("Noop.");
                        break;
                    case 3:
                    {
                        var (x1, y1, z1) = (float.Parse(subArgs[0]), float.Parse(subArgs[1]), float.Parse(subArgs[2]));
                        var prev = TargetObject.transform.rotation.eulerAngles;
                        var toAdd = new Vector3(x1, y1, z1);
                        TargetObject.transform.rotation = Quaternion.Euler(prev + toAdd);
                        RA($"Rotated object {TargetObject.name}\nOldRot: {prev}\nNewRot: {TargetObject.transform.rotation.eulerAngles}");
                        TargetObject.NotifyPlayers();
                    }
                        break;
                }
            }
            catch (Exception e)
            {
                RA($"Failed:{e.Message}\n{e.StackTrace}");
            }
        }

        public void HandleScl(string[] args)
        {
            var subArgs = args.Skip(2).ToList();
            try
            {
                switch (subArgs.Count)
                {
                    case 0: {
                        RA($"Scale: {TargetObject.transform.localScale}");
                    }break;
                    case 1:
                    {
                        var scale = float.Parse(subArgs[0]);
                        var prev = TargetObject.transform.localScale.ToString();
                        var toAdd = Vector3.one * scale;
                        TargetObject.transform.localScale += toAdd;
                        RA($"Scaled object {TargetObject.name}\nOldScl: {prev}\nNewScl: {TargetObject.transform.localScale}");
                        TargetObject.NotifyPlayers();
                    }break;
                    case 2:
                    {
                        var (x1,y1) = (float.Parse(subArgs[0]), float.Parse(subArgs[1]));
                        var prev = TargetObject.transform.localScale.ToString();
                        var toAdd = new Vector3(x1, y1);
                        TargetObject.transform.localScale += toAdd;
                        RA($"Scaled object {TargetObject.name}\nOldScl: {prev}\nNewScl: {TargetObject.transform.localScale}");
                        TargetObject.NotifyPlayers();
                    }break;
                    case 3:
                    {
                        var (x1,y1,z1) = (float.Parse(subArgs[0]), float.Parse(subArgs[1]), float.Parse(subArgs[2]));
                        var prev = TargetObject.transform.localScale.ToString();
                        var toAdd = new Vector3(x1,y1,z1);
                        TargetObject.transform.localScale += toAdd;
                        RA($"Scaled object {TargetObject.name}\nOldScl: {prev}\nNewScl: {TargetObject.transform.localScale}");
                        TargetObject.NotifyPlayers();
                    }break;
                }
            }
            catch (Exception e)
            {
                RA($"Failed:{e.Message}\n{e.StackTrace}");
            }
        }
            
        #endregion
        
        #region CmdHandler

        public bool HandleArgs(string[] args)
        {
            switch (args[1])
            {
                case "cnt":
                case "center":
                    _center = args.Length == 2 ? 
                        BuilderPlayer.GameObject.transform.position.Copy() : 
                        new Vector3(float.Parse(args[2]), float.Parse(args[3]), float.Parse(args[4]));
                    return true;
                case "mv":
                case "mov":
                case "pos":
                case "position":
                    HandlePos(args);
                    return true;
                case "rot":
                case "rotation":
                    HandleRot(args);
                    return true;
                case "scl":
                case "scale":
                    HandleScl(args);
                    return true;
                case "mult": {
                    var pre = _mult;
                    _mult = float.Parse(args[2]);
                    Hint($"Mult\nPre: {pre}\nNew: {_mult}");
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}