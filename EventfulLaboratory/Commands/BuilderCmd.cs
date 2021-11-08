using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using EventfulLaboratory.Extension;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Permissions.Extensions;
using MEC;
using Mirror;
using UnityEngine;
using Object = UnityEngine.Object;
using static EventfulLaboratory.Util;

//KDE2LjIsIDEwMDcuNSwgLTU5LjIpJUhDWiBCcmVha2FibGVEb29yKENsb25lKXwoMTYuMiwgMTAwNy41LCAtNTkuMil8KDAuNSwgMC41LCAtMC41LCAwLjUpfCgxLjAsIDEuMCwgMS4wKSVIQ1ogQnJlYWthYmxlRG9vcihDbG9uZSl8KDE2LjIsIDEwMDcuOSwgLTU4LjApfCgwLjUsIC0wLjUsIDAuNSwgLTAuNSl8KDAuNCwgMC43LCAxLjApJUhDWiBCcmVha2FibGVEb29yKENsb25lKXwoMTkuNSwgMTAwNy45LCAtNjAuNSl8KDAuNSwgLTAuNSwgLTAuNSwgMC41KXwoMC40LCAwLjcsIDEuMCklSENaIEJyZWFrYWJsZURvb3IoQ2xvbmUpfCgxOS42LCAxMDA3LjksIC01OC4yKXwoMC4wLCAwLjAsIDAuNywgMC43KXwoMC40LCAxLjAsIDEuMCklSENaIEJyZWFrYWJsZURvb3IoQ2xvbmUpfCgxNi4xLCAxMDA3LjksIC02MC4yKXwoMC4wLCAwLjAsIDAuNywgLTAuNyl8KDAuNCwgMS4wLCAxLjApJUhDWiBCcmVha2FibGVEb29yKENsb25lKXwoMTUuMywgMTAwOC41LCAtNjAuNyl8KDAuNSwgMC41LCAtMC41LCAwLjUpfCgwLjUsIDEuNSwgMS4wKSVIQ1ogQnJlYWthYmxlRG9vcihDbG9uZSl8KDIwLjMsIDEwMDguNSwgLTU3LjcpfCgwLjUsIC0wLjUsIC0wLjUsIC0wLjUpfCgwLjUsIDEuNSwgMS4wKSVIQ1ogQnJlYWthYmxlRG9vcihDbG9uZSl8KDE1LjMsIDEwMDguNSwgLTU5LjIpfCgwLjUsIDAuNSwgLTAuNSwgMC41KXwoMS4wLCAwLjMsIDEuMCklSENaIEJyZWFrYWJsZURvb3IoQ2xvbmUpfCgxOS4zLCAxMDA4LjUsIC01OS4yKXwoMC41LCAwLjUsIC0wLjUsIDAuNSl8KDEuMCwgMC4zLCAxLjApJUhDWiBCcmVha2FibGVEb29yKENsb25lKXwoMTUuMSwgMTAwOS4xLCAtNjEuMil8KDAuMCwgMC4wLCAwLjcsIC0wLjcpfCgwLjQsIDEuNiwgMS4wKSVIQ1ogQnJlYWthYmxlRG9vcihDbG9uZSl8KDIwLjYsIDEwMDkuMSwgLTU3LjIpfCgwLjcsIC0wLjcsIDAuMCwgMC4wKXwoMC40LCAxLjYsIDEuMCklSENaIEJyZWFrYWJsZURvb3IoQ2xvbmUpfCgyMC4yLCAxMDA5LjEsIC02MS40KXwoMC41LCAtMC41LCAtMC41LCAwLjUpfCgwLjQsIDEuMiwgMS4wKSVIQ1ogQnJlYWthYmxlRG9vcihDbG9uZSl8KDE1LjIsIDEwMDkuMSwgLTU3LjEpfCgwLjUsIC0wLjUsIDAuNSwgLTAuNSl8KDAuNCwgMS4yLCAxLjAp

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
                    case "prefabs":
                        var str = "Prefabs:";
                        NetworkManager.singleton.spawnPrefabs.ForEach(prefab =>
                        {
                            str += prefab.name + ": - " + prefab.gameObject.name;
                        });
                        player.RemoteAdminMessage(str);
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
                response = $"Builder cmd failed: {e.Message}\n{e}";
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
                switch (ev.Shooter.CurrentItem.Type)
                {
                    case ItemType.GunRevolver: {
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
                    case ItemType.GunCrossvec: {
                        var ray = ev.Shooter.Raytrace();
                        if (ray.IsHit())
                        {
                            _builtPlatformKeeper.Add(BuilderUtil.JustFuckingSpawnADoor(ray.point + new Vector3(0, 0.5f, 0f)));
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
                    case ItemType.GunLogicer: {
                        if (TargetObject != null)
                        {
                            _builtPlatformKeeper.Remove(TargetObject);
                            Object.Destroy(TargetObject);
                            Hint("Target object deleted.");
                        }
                        else
                        {
                            Hint($"No selected object.");
                        }
                    }break;
                    case ItemType.GunShotgun: {
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
                                foreach (var componentsInChild in thingie.GetComponentsInChildren<Material>())
                                {
                                    componentsInChild.color = Color.red;
                                }
                            }
                            else
                            {
                                Hint("Miss or not Movable.");
                            }
                            
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

        /*private void MovementEvent(SyncingDataEventArgs ev)
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
        }*/
        
        #region BuilderPlayer Setup 
        private static readonly ItemType[] _weapons =
        {
            ItemType.GunRevolver,
            ItemType.GunCrossvec,
            ItemType.GunCOM15,
            ItemType.GunLogicer,
            ItemType.GunShotgun,
            ItemType.GunE11SR,
            //ItemType.KeycardO5
        };

        public static IEnumerator<float> SetupBuilder(Player player)
        {
            player.SetRole(RoleType.Tutorial);
            yield return Timing.WaitForSeconds(0.5f);
            player.NoClipEnabled = true;
            player.IsGodModeEnabled = true;
            foreach (var ammoKey in player.Ammo.Keys)
            {
                player.Ammo[ammoKey] = 3000;
            }
            
            foreach (var itemType in _weapons)
            {
                player.AddItem(itemType);
            }
        }
        #endregion

        private Vector3 ArgsToVector3(List<string> subArgs)
        {
            switch (subArgs.Count)
            {
                case 1: return Vector3.one * float.Parse(subArgs[0]);
                case 2: return new Vector3(float.Parse(subArgs[0]), float.Parse(subArgs[1]));
                case 3: return new Vector3(float.Parse(subArgs[0]),float.Parse(subArgs[1]),float.Parse(subArgs[2]));
            }

            return Vector3.zero;
        }
        
        #region PosRotScl Handlers

        private void PerformPosChange(Vector3 pos, bool toAdd = true, GameObject gameObject = null, bool showLog = true)
        {
            if (gameObject == null) gameObject = TargetObject;
            
            var prev = gameObject.transform.position.ToString();
            
            if (toAdd)
                gameObject.transform.position += pos;
            else
                gameObject.transform.position = pos;
            if (showLog) RA($"Moved object {gameObject.name}\nOldPos: {prev}\nNewPos: {gameObject.transform.position}");
            gameObject.NotifyPlayers();
        }
        
        private void PerformRotChange(Vector3 rot, bool toAdd = true, GameObject gameObject = null, bool showLog = true)
        {
            if (gameObject == null) gameObject = TargetObject;
            
            var prev = gameObject.transform.rotation.ToString();
            
            if (toAdd)
                gameObject.transform.rotation = Quaternion.Euler(rot + gameObject.transform.rotation.eulerAngles);
            else
                gameObject.transform.rotation = Quaternion.Euler(rot);
            if (showLog) RA($"Rotated object {gameObject.name}\nOldRot: {prev}\nNewRot: {gameObject.transform.rotation.eulerAngles}");
            gameObject.NotifyPlayers();
        }
        
        private void PerformSclChange(Vector3 scl, bool toAdd = true, GameObject gameObject = null, bool showLog = true)
        {
            if (gameObject == null) gameObject = TargetObject;
            
            var prev = gameObject.transform.localScale.ToString();
            
            if (toAdd)
                gameObject.transform.localScale += scl;
            else
                gameObject.transform.localScale = scl;
            
            if (showLog) RA($"Scaled object {gameObject.name}\nOldScl: {prev}\nNewScl: {gameObject.transform.localScale}");
            gameObject.NotifyPlayers();
        }

        private void HandlePos(string[] args)
        {
            var subArgs = args.Skip(2).ToList();
            if (subArgs.Count == 0)
            {
                RA($"Pos: {TargetObject.transform.position}");
                return;
            }
            try
            {
                PerformPosChange(ArgsToVector3(subArgs));
            }
            catch (Exception e)
            {
                RA($"Failed:{e.Message}\n{e.StackTrace}");
            }
        }
        
        private void SetPosition(string[] args)
        {
            var subArgs = args.Skip(2).ToList();
            if (subArgs.Count == 0)
            {
                RA($"Pos: {TargetObject.transform.position}");
                return;
            }
            try
            {
                PerformPosChange(ArgsToVector3(subArgs), false);
            }
            catch (Exception e)
            {
                RA($"Failed:{e.Message}\n{e.StackTrace}");
            }
        }

        private void HandleRot(string[] args)
        {
            var subArgs = args.Skip(2).ToList();
            if (subArgs.Count == 0)
            {
                RA($"Rot: {TargetObject.transform.rotation.eulerAngles}");
                return;
            }

            if (subArgs.Count == 2) return; //Noop
            
            try
            {
                PerformRotChange(ArgsToVector3(subArgs), false);
            }
            catch (Exception e)
            {
                RA($"Failed:{e.Message}\n{e.StackTrace}");
            }
        }

        private void HandleScl(string[] args)
        {
            var subArgs = args.Skip(2).ToList();
            if (subArgs.Count == 0)
            {
                RA($"Scale: {TargetObject.transform.localScale}");
                return;
            }
            
            try
            {
                PerformSclChange(ArgsToVector3(subArgs), false);
            }
            catch (Exception e)
            {
                RA($"Failed:{e.Message}\n{e.StackTrace}");
            }
        }

        private void HandleGPos(string[] args)
        {
            var subArgs = args.Skip(2).ToList();
            if (subArgs.Count == 0)
            {
                RA($"Noop.");
                return;
            }
            
            Vector3 toRotate = ArgsToVector3(subArgs);
            foreach (var gameObject in _builtPlatformKeeper)
            {
                PerformPosChange(toRotate, true, gameObject, false);
            }
            RA($"Mass Moved Pos. Added: {toRotate}");
        }
        
        private void HandleGRot(string[] args)
        {
            RA("WIP.");/*
            
            var subArgs = args.Skip(2).ToList();
            if (subArgs.Count == 0 || subArgs.Count > 2)
            {
                RA($"Noop.");
                return;
            }

            float angle = float.Parse(args[0]);
            foreach (var gameObject in _builtPlatformKeeper)
            {
                
            }
            RA($"Mass Moved Pos. Added: {addPos}");*/
            
        }

        private void HandleGScl(string[] args)
        {
            var subArgs = args.Skip(2).ToList();
            if (subArgs.Count == 0)
            {
                RA($"Noop.");
                return;
            }
            
            Vector3 toScale = ArgsToVector3(subArgs);
            Vector3 cntPoint = CenterPoint();
            foreach (var gameObject in _builtPlatformKeeper)
            {
                PerformPosChange((gameObject.transform.position - cntPoint).ScaleStatic(toScale), true, gameObject, false);
                PerformSclChange(gameObject.transform.localScale.ScaleStatic(toScale), true, gameObject, false);
            }

            RA($"Mass Scaled. Scale: {toScale}");
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
                    HandlePos(args);
                    return true;
                case "pos":
                case "position":
                    SetPosition(args);
                    return true;
                case "gmv":
                case "gmov":
                case "gpos":
                case "gposition":
                    HandleGPos(args);
                    return true;
                case "rot":
                case "rotation":
                    HandleRot(args);
                    return true;
                case "grot":
                case "grotation":
                    HandleGRot(args);
                    return true;
                case "scl":
                case "scale":
                    HandleScl(args);
                    return true;
                case "gscl":
                case "gscale":
                    HandleGScl(args);
                    return true;
                case "door":
                    var newDoor = BuilderUtil.JustFuckingSpawnADoor(_center);
                    _builtPlatformKeeper.Add(newDoor);
                    TargetObject = newDoor;
                    RA($"Added door to {_center}");
                    return true;
                case "serialize":
                    Logger.Info((_center + "%" + _builtPlatformKeeper.Aggregate("", (s, o) => o == null ? s : s + "%" + o.BuilderSerialize()).Substring(1)).ToBase64());
                    RA("Serialized. Check Console");
                    return true;
                case "load":
                    var str = args[2].FromBase64();
                    var parts = str.Split('%');
                    _center = parts[0].ParseVec3();
                    for (int i = 1; i < parts.Length; i++)
                    {
                        RA($"4/{i}");
                        var part = parts[i].Split('|');
                        var obj = BuilderUtil.HandleSpawning(
                            part[0],
                            part[1].ParseVec3(),
                            part[2].ParseQuat(),
                            part[3].ParseVec3()
                        );
                        _builtPlatformKeeper.Add(obj);
                    }
                    return true;
                case "clear":
                    foreach (var o in _builtPlatformKeeper)
                    {
                        Object.Destroy(o);
                    }
                    _builtPlatformKeeper.Clear();
                    RA("Cleared platform list");
                    return true;
                case "copy":
                {
                    var newObj = BuilderUtil.HandleSpawning(
                        TargetObject.name,
                        TargetObject.transform.position,
                        TargetObject.transform.rotation,
                        TargetObject.transform.localScale
                    );
                    if (newObj == null)
                    {
                        RA("Object copy Failed. Returned null");
                        return true;
                    }

                    _builtPlatformKeeper.Add(newObj);
                    TargetObject = newObj;
                    RA("Copied object");
                    return true;
                }
                case "spawn":
                {
                    var newObj = BuilderUtil.HandleSpawning(
                        args[2],
                        BuilderPlayer.GameObject.transform.position,
                        Quaternion.identity,
                        Vector3.one
                    );
                    
                    if (newObj == null)
                    {
                        RA("Object spawn Failed. Returned null");
                        return true;
                    }

                    _builtPlatformKeeper.Add(newObj);
                    TargetObject = newObj;
                    RA("Spawned object " + newObj.name);
                    return true;
                }
            }

            return false;
        }
        #endregion

        private Vector3 CenterPoint() => _builtPlatformKeeper.CenterPoint();
    }
}