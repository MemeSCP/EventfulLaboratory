using System;
using System.Collections.Generic;
using System.Linq;

using EventfulLaboratory.structs;

using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs;

using MEC;

using Server = Exiled.Events.Handlers.Server;

namespace EventfulLaboratory.slevents
{
    public class TeamWarfare : AEvent
    {
        private static Random _rng;

        private static ItemType _roundWeapon;

        private static readonly int[] _attachments = {0, 0, 0};

        private static readonly ItemType[] _spawnItems =
        {
            ItemType.Adrenaline,
            ItemType.Medkit,
            ItemType.GrenadeFrag
        };

        private static readonly ItemType[] _weapons =
        {
            ItemType.GunLogicer,
            ItemType.GunProject90,
            ItemType.GunMP7,
            ItemType.GunCOM15,
            ItemType.GunE11SR,
            ItemType.GunUSP
        };

        private static int _maxScore = 50;
        private int _chaosKills;

        private Dictionary<int, Tuple<int, int>> _kda;

        private int _ntfKills;


        public override void OnNewRound()
        {
            _rng = new Random();
            _roundWeapon = _weapons[_rng.Next(_weapons.Length)];
            _attachments[0] = _rng.Next(3);
            _attachments[1] = _rng.Next(4);
            _attachments[2] = _rng.Next(4);
            _kda = new Dictionary<int, Tuple<int, int>>();

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
                Timing.RunCoroutine(SpawnHubAsParameter(player,
                    player.Id % 2 == 1 ? RoleType.NtfLieutenant : RoleType.ChaosInsurgency));
                player.Broadcast(5, "Welcome to TeamWarfare! First team to get " + _maxScore + " kills wins the round!");
            }

            Common.DisableElevators();

            Exiled.Events.Handlers.Player.Died += CountAndRespawnKills;
            Exiled.Events.Handlers.Player.Joined += OnPlayerJoin;
        }

        private void OnPlayerJoin(JoinedEventArgs ev)
        {
            SpawnPlayer(ev.Player);
        }

        private void OnRoundRestart()
        {
            OnRoundEnd();
        }

        public override void OnRoundEnd()
        {
            Exiled.Events.Handlers.Player.Died -= CountAndRespawnKills;
            Exiled.Events.Handlers.Player.Joined -= OnPlayerJoin;
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
                if (ev.Killer.Role == RoleType.ChaosInsurgency)
                    _chaosKills++;
                else
                    _ntfKills++;
            }

            foreach (var player in Player.List)
                if (player.Role == RoleType.Spectator)
                    Timing.RunCoroutine(SpawnHubAsParameter(player,
                        player.Id % 2 == 1 ? RoleType.NtfLieutenant : RoleType.ChaosInsurgency));

            if (_chaosKills >= _maxScore || _ntfKills >= _maxScore)
            {
                if (_chaosKills > _ntfKills)
                {
                    Common.Broadcast(30, "Chaos Wins!", true);
                    Common.LockRound(false);
                    Common.ForceEndRound(RoleType.ChaosInsurgency);
                }
                else if (_chaosKills < _ntfKills)
                {
                    Common.Broadcast(30, "NTF Wins!", true);
                    Common.LockRound(false);
                    Common.ForceEndRound(RoleType.NtfCommander);
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
                Timing.RunCoroutine(SpawnHubAsParameter(ev.Target, ev.Target.Role));
            }
        }

        private IEnumerator<float> SpawnHubAsParameter(Player player, RoleType role)
        {
            yield return Timing.WaitForSeconds(0.3f);
            player.SetRole(role);
            yield return Timing.WaitForSeconds(0.3f);
            player.ClearInventory();
            yield return Timing.WaitForSeconds(0.1f);
            foreach (var item in _spawnItems) player.AddItem(item);
            player.Ammo[(int) AmmoType.Nato9] = 3000;
            player.Ammo[(int) AmmoType.Nato556] = 3000;
            player.Ammo[(int) AmmoType.Nato762] = 3000;

            //TODO: Attachments blacklist
            var sif = new Inventory.SyncItemInfo
            {
                id = _roundWeapon,
                durability = _roundWeapon == ItemType.GunLogicer ? 100f : 30f,
                modBarrel = _attachments[0],
                modOther = _attachments[1],
                modSight = _attachments[2]
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
                _kda.Add(userid, kill ? new Tuple<int, int>(1, 0) : new Tuple<int, int>(0, 1));
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

        private void UpdateKdaOfUser(Player player)
        {
            string kdaString;
            if (_kda.ContainsKey(player.Id))
            {
                var kda = _kda[player.Id];
                kdaString = $"/{kda.Item1}|{kda.Item2}/";
            }
            else
            {
                kdaString = "/0|0/";
            }

            var text = player.RankName ?? "";
            if (text.Contains("/")) text = text.Split('/')[2];
            player.RankName = $"{kdaString} {text}";
        }

        private void SpawnPlayer(Player player)
        {
            Timing.RunCoroutine(SpawnHubAsParameter(player,
                player.Id % 2 == 1 ? RoleType.NtfLieutenant : RoleType.ChaosInsurgency));
            player.Broadcast(5, "First team to get " + _maxScore + " kills win the round!");
        }
    }
}