using System.Collections.Generic;
using System.Linq;
using CameraShaking;
using EventfulLaboratory.Extension;
using EventfulLaboratory.Handler;
using EventfulLaboratory.structs;

using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using MEC;
using UnityEngine;
using UnityEngine.UI;
using Firearm = Exiled.API.Features.Items.Firearm;
using Random = System.Random;
using Server = Exiled.Events.Handlers.Server;

namespace EventfulLaboratory.slevents
{
    public class TeamWarfare : AEvent
    {
        private const RoleType _team1 = RoleType.NtfCaptain;
        private const RoleType _team2 = RoleType.ChaosRifleman;

        private static Random _rng;

        private static ItemType _roundWeapon;

        private static readonly int[] _attachments = {0, 0, 0};

        private static readonly ItemType[] _spawnItems =
        {
            ItemType.Adrenaline,
            ItemType.Medkit,
            ItemType.GrenadeHE
        };

        private static readonly ItemType[] _weapons =
        {
            ItemType.GunCrossvec,
            ItemType.GunLogicer,
            ItemType.GunRevolver,
            ItemType.GunShotgun,
            ItemType.GunAK,
            ItemType.GunCOM15,
            ItemType.GunCOM18,
            ItemType.GunE11SR,
            ItemType.GunFSP9,
        };

        private static int _maxScore = 50;
        
        private int _chaosKills;
        private int _ntfKills;

        private Dictionary<int, KdaHolder> _kda;

        private EvenTeamSplitHandler _teamHandler;
        
        public override void OnNewRound()
        {
            _rng = new Random();
            _roundWeapon = _weapons[_rng.Next(_weapons.Length)];
            /*_attachments[0] = _rng.Next(3);
            _attachments[1] = _rng.Next(4);
            _attachments[2] = _rng.Next(4);*/
            _kda = new Dictionary<int, KdaHolder>();

            _teamHandler = new EvenTeamSplitHandler(_team1, _team2, true);

            Server.RestartingRound += OnRoundRestart;
        }

        public override void OnRoundStart()
        {
            _maxScore = Player.List.ToList().Count * 2 + 10;
            Common.LockRound();
            _ntfKills = 0;
            _chaosKills = 0;
            
            foreach (var player in Player.List)
            {
                Timing.RunCoroutine(SpawnPlayer(player));
                player.Broadcast(5, "Welcome to TeamWarfare! First team to get " + _maxScore + " kills wins the round!");
            }

            Common.DisableElevators();

            Exiled.Events.Handlers.Player.Died += CountAndRespawnKills;
            Exiled.Events.Handlers.Player.Joined += OnPlayerJoin;
            Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
        }

        private void OnPlayerJoin(JoinedEventArgs ev)
        {
            SpawnPlayer(ev.Player);
        }

        private void OnPlayerLeft(LeftEventArgs ev)
        {
            _teamHandler.SetDirty();
        }

        private void OnRoundRestart()
        {
            OnRoundEnd();
        }

        public override void OnRoundEnd()
        {
            Exiled.Events.Handlers.Player.Died -= CountAndRespawnKills;
            Exiled.Events.Handlers.Player.Joined -= OnPlayerJoin;
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeft;
        }

        private string FormatScore()
        {
            return "<color=green>Chaos:</color> " + _chaosKills + " <color=red>||</color> <color=blue>NTF:</color> " + _ntfKills;
        }


        private void CountAndRespawnKills(DiedEventArgs ev)
        {
            ev.Target.ClearInventory();
            if (ev.Killer.Role != ev.Target.Role)
            {
                AddToKda(ev.Killer.Id);
                UpdateKdaOfUser(ev.Killer);
                AddToKda(ev.Target.Id, false);
                UpdateKdaOfUser(ev.Target);
                if (ev.Killer.Role == RoleType.ChaosRifleman)
                    _chaosKills++;
                else
                    _ntfKills++;
            }

            foreach (var player in Player.List)
                if (player.Role == RoleType.Spectator)
                    Timing.RunCoroutine(SpawnPlayer(player));

            if (_chaosKills >= _maxScore || _ntfKills >= _maxScore)
            {
                if (_chaosKills > _ntfKills)
                {
                    Common.Broadcast(30, "Chaos Wins!", true);
                    Common.LockRound(false);
                    Common.ForceEndRound(RoleType.ChaosRifleman);
                }
                else if (_chaosKills < _ntfKills)
                {
                    Common.Broadcast(30, "NTF Wins!", true);
                    Common.LockRound(false);
                    Common.ForceEndRound(RoleType.NtfCaptain);
                }
                else
                {
                    Common.Broadcast(30, "Tie?", true);
                    Common.LockRound(false);
                    Common.ForceEndRound(RoleType.None);
                }
            }
            else
            {
                Common.Broadcast(60, FormatScore(), true);
                Timing.RunCoroutine(SpawnPlayer(ev.Target));
            }
        }

        private IEnumerator<float> SpawnPlayer(Player player)
        {
            yield return Timing.WaitForSeconds(0.1F);
            
            Timing.WaitUntilDone(_teamHandler.SpawnPlayer(player));
            
            yield return Timing.WaitForSeconds(0.1f);
            foreach (var item in _spawnItems) player.AddItem(item);
            foreach (var ammoKey in player.Ammo.Keys)
            {
                player.Ammo[ammoKey] = 3000;
            }
            
            var sif = new Firearm(_roundWeapon)
            {
                /*Attachments = new []
                {
                    new FirearmAttachment
                    {
                        Name = AttachmentNameTranslation.Flashlight,
                        Settings = new AttachmentSettings()
                        {
                            PhysicalLength = 20,
                            Weight = 20000
                        },
                        Slot = AttachmentSlot.Sight
                    },
                    new FirearmAttachment
                    {
                        Name = AttachmentNameTranslation.Laser,
                        Settings = new AttachmentSettings()
                        {
                            PhysicalLength = 30,
                            Weight = -2
                        },
                        Slot = AttachmentSlot.Ammunition
                    }
                },*/
                Ammo = 30,
                FireRate = 3,
                Recoil = new RecoilSettings()
                {
                    UpKick = 3
                },
                Scale = new Vector3(1, 1, 1)
            };
            player.AddItem(sif);
            player.IsGodModeEnabled = true;
            yield return Timing.WaitForSeconds(5);
            player.IsGodModeEnabled = false;
        }

        private void AddToKda(int userid, bool kill = true)
        {
            if (!_kda.ContainsKey(userid))
            {
                _kda.Add(userid, kill ? new KdaHolder(1, 0) : new KdaHolder(0,1 ));
            }
            else
            {
                if (kill)
                    _kda[userid].AddKill();
                else
                    _kda[userid].AddDeath();
            }
        }

        private void UpdateKdaOfUser(Player player)
        {
            if (EventfulLab.Instance.Config.EnableTagModifications)
            {
                string kdaString;
                if (_kda.ContainsKey(player.Id))
                {
                    kdaString = _kda[player.Id].ToString();
                }
                else
                {
                    kdaString = "/0|0/";
                }

                var text = player.RankName ?? "";
                if (text.Contains("/")) text = text.Split('/')[2];
                player.RankName = $"{kdaString} {text.TrimStart()}";
                player.UpdateRankColorToRole();
            }
        }
    }
    
    internal class KdaHolder
    {
        public int Kill { get; private set; }
        public int Death { get; private set; }

        public KdaHolder(int kill, int death)
        {
            Kill = kill;
            Death = death;
        }

        public int AddKill(int amount = 1) => Kill += amount;
        public int AddDeath(int amount = 1) => Death += amount;

        public override string ToString() => $"/{Kill}/{Death}";
    }
}