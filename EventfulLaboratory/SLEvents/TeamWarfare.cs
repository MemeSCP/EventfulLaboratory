using System;
using System.Collections.Generic;
using System.Linq;
using EventfulLaboratory.structs;
using EXILED;
using EXILED.ApiObjects;
using EXILED.Extensions;
using MEC;
using UnityEngine.Assertions.Must;

namespace EventfulLaboratory.slevents
{
    public class TeamWarfare : AEvent
    {

        private static Random _rng;

        private static ItemType _roundWeapon;
        private static int[] attachments = {0, 0, 0};

        private static readonly ItemType[] SpawnItems = new[]
        {
            ItemType.Adrenaline,
            ItemType.Medkit,
            ItemType.GrenadeFrag
        };

        private static readonly ItemType[] Weapons = new[]
        {
            ItemType.GunLogicer,
            ItemType.GunProject90,
            ItemType.GunMP7,
            ItemType.GunCOM15,
            ItemType.GunE11SR,
            ItemType.GunUSP,
        };

        private int _ntfKills = 0;
        private int _chaosKills = 0;
        private static int _maxScore = 50;

        private Dictionary<int, Tuple<int, int>> _kda;
        
        
        public override void OnNewRound()
        {
            _rng = new Random();
            _roundWeapon = Weapons[_rng.Next(Weapons.Length)];
            attachments[0] = _rng.Next(3);
            attachments[1] = _rng.Next(4);
            attachments[2] = _rng.Next(4);
            _kda = new Dictionary<int, Tuple<int, int>>();

            Events.RoundRestartEvent += RoundRestartEvent;
        }

        public override void OnRoundStart()
        {
            _maxScore = Player.GetHubs().ToList().Count * 2 + 10;
            Common.LockRound();
            _ntfKills = 0;
            _chaosKills = 0;
            foreach (ReferenceHub hub in Player.GetHubs())
            {
                Timing.RunCoroutine(SpawnHubAsParameter(hub,
                    (hub.GetPlayerId() % 2 == 1 ? RoleType.NtfLieutenant : RoleType.ChaosInsurgency)));
                hub.Broadcast(5, "First team to get " + _maxScore + " kills win the round!", false);
            }
            Map.DetonateNuke();
            Map.NukeDetonationTimer = 2f;
            Map.StartNuke();
            Map.StartDecontamination();

            Events.PlayerDeathEvent += CountAndRespawnKills;
            Events.PlayerJoinEvent += OnPlayerJoin;
        }

        private void OnPlayerJoin(PlayerJoinEvent ev)
        {
            Timing.RunCoroutine(SpawnHubAsParameter(ev.Player,
                (ev.Player.GetPlayerId() % 2 == 1 ? RoleType.NtfLieutenant : RoleType.ChaosInsurgency)));
            ev.Player.Broadcast(5, "First team to get " + _maxScore + " kills win the round!", false);
        }

        public void RoundRestartEvent()
        {
            OnRoundEnd();
        }

        public override void OnRoundEnd()
        {
            Events.PlayerDeathEvent -= CountAndRespawnKills;
            Events.PlayerJoinEvent -= OnPlayerJoin;
        }

        public override void Enable()
        {
            
        }

        public override void Disable()
        {
            
        }

        public override void Reload()
        {
            
        }

        private String FormatScore() => "<color=green>Chaos:</color> " + _chaosKills + " <color=red>||</color> <color=blue>NTF:</color> " + _ntfKills;
        

        private void CountAndRespawnKills(ref PlayerDeathEvent ev)
        {
            ev.Player.ClearInventory();
            if (ev.Killer.GetRole() != ev.Player.GetRole())
            {
                AddToKda(ev.Killer.GetPlayerId());
                UpdateKDAOfUser(ev.Killer);
                AddToKda(ev.Player.GetPlayerId(), false);
                UpdateKDAOfUser(ev.Player);
                if (ev.Killer.GetRole() == RoleType.ChaosInsurgency)
                {
                    _chaosKills++;
                }
                else
                {
                    _ntfKills++;
                }
            }
            
            foreach (ReferenceHub hub in Player.GetHubs())
            {
                if (hub.GetRole() == RoleType.Spectator)
                {
                    Timing.RunCoroutine(SpawnHubAsParameter(hub,
                        (hub.GetPlayerId() % 2 == 1 ? RoleType.NtfLieutenant : RoleType.ChaosInsurgency)));
                }
            }

            if (_chaosKills >= _maxScore || _ntfKills >= _maxScore)
            {
                bool force = true, allow = true, something = false;
                RoundSummary.LeadingTeam winner = RoundSummary.LeadingTeam.Draw;
                if (_chaosKills > _ntfKills)
                {
                    Common.Broadcast(30, "Chaos Wins!", true);
                    winner = RoundSummary.LeadingTeam.ChaosInsurgency;

                } else if (_chaosKills < _ntfKills)
                {
                    Common.Broadcast(30, "NTF Wins!", true);
                    winner = RoundSummary.LeadingTeam.FacilityForces;
                }
                else
                {
                    Common.Broadcast(30, "Tie?", true);
                }
                Map.RoundLock = false;
                Events.InvokeCheckRoundEnd(ref force, ref allow, ref winner, ref something);
            }
            else
            {
                Common.Broadcast(60, FormatScore(), true);
                Timing.RunCoroutine(SpawnHubAsParameter(ev.Player, ev.Player.GetRole()));
            }
        }

        private IEnumerator<float> SpawnHubAsParameter(ReferenceHub player, RoleType role)
        {
            yield return Timing.WaitForSeconds(0.3f);
            player.SetRole(role);
            yield return Timing.WaitForSeconds(0.3f);
            player.ClearInventory();
            yield return Timing.WaitForSeconds(0.1f);
            foreach (ItemType item in SpawnItems)
            {
                player.AddItem(item);
            }
            player.ammoBox.SetOneAmount(0, "300");
            player.ammoBox.SetOneAmount(1, "300");
            player.ammoBox.SetOneAmount(2, "300");
            
            Inventory.SyncItemInfo sif = new Inventory.SyncItemInfo
            {
                id = _roundWeapon,
                durability = _roundWeapon == ItemType.GunLogicer ? 100f : 30f,
                modBarrel = attachments[0],
                modOther = attachments[1],
                modSight = attachments[2],
            };
            player.AddItem(sif);
        }

        private void AddToKda(int userid, bool kill = true)
        {
            if (!_kda.ContainsKey(userid))
            {
                _kda.Add(userid, kill ? new Tuple<int, int>(1,0) : new Tuple<int, int>(0,1));
            }
            else
            {
                //TODO: Test & rework
                var (kills, deaths) = _kda[userid].ToValueTuple();
                if (kill)
                    kills++;
                else
                    deaths++;
                _kda[userid] = new Tuple<int, int>(kills, deaths);
            }
        }

        private void UpdateKDAOfUser(ReferenceHub hub)
        {
            String KDAString;
            if (_kda.ContainsKey(hub.GetPlayerId()))
            {
                Tuple<int, int> kda = _kda[hub.GetPlayerId()];
                KDAString = $"[<color=green>{kda.Item1}</color>|<color=red>{kda.Item2}</color>]";
            }
            else
            {
                KDAString = "[<color=green>0</color>|<color=red>0</color>]";
            }
            hub.serverRoles.SetText($"{KDAString} {hub.serverRoles.MyText}");
        }
    }
}