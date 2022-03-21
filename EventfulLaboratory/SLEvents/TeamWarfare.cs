using System.Collections.Generic;
using System.Linq;
using EventfulLaboratory.Extension;
using EventfulLaboratory.Handler;
using EventfulLaboratory.structs;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs;
using InventorySystem.Items.Firearms.Attachments;
using MEC;
using Firearm = Exiled.API.Features.Items.Firearm;
using Server = Exiled.Events.Handlers.Server;

namespace EventfulLaboratory.SLEvents
{
    public class TeamWarfare : AEvent
    {
        private const RoleType _team1 = RoleType.NtfCaptain;
        private const RoleType _team2 = RoleType.ChaosRifleman;

        private static ItemType _roundWeapon;
        private static uint _attachmentCode;

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
            _roundWeapon = _weapons[Util.Random.Next(_weapons.Length)];
            _attachmentCode = AttachmentsUtils.GetRandomAttachmentsCode(_roundWeapon);
            
            _kda = new Dictionary<int, KdaHolder>();

            _teamHandler = new EvenTeamSplitHandler(_team1, _team2, true);

            Server.RestartingRound += OnRoundRestart;
            
            Log.Info($"Round weapon: {_roundWeapon}");
        }

        public override void OnRoundStart()
        {
            _maxScore = Player.List.ToList().Count * 2 + 10;
            Util.RoundUtils.LockRound();
            _ntfKills = 0;
            _chaosKills = 0;
            
            foreach (var player in Player.List)
            {
                Timing.RunCoroutine(SpawnPlayer(player));
                player.Broadcast(5, "Welcome to TeamWarfare! First team to get " + _maxScore + " kills wins the round!");
            }

            Util.MapUtil.DisableElevators();

            Exiled.Events.Handlers.Player.Died += CountAndRespawnKills;
            Exiled.Events.Handlers.Player.Joined += OnPlayerJoin;
            Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
        }

        private void OnPlayerJoin(JoinedEventArgs ev) => Timing.RunCoroutine(SpawnPlayer(ev.Player));

        private void OnPlayerLeft(LeftEventArgs ev) => _teamHandler.SetDirty();

        private void OnRoundRestart() => OnRoundEnd();

        public override void OnRoundEnd()
        {
            Exiled.Events.Handlers.Player.Died -= CountAndRespawnKills;
            Exiled.Events.Handlers.Player.Joined -= OnPlayerJoin;
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeft;
        }

        private string FormatScore() => "<color=green>Chaos:</color> " + _chaosKills + " <color=red>||</color> <color=blue>NTF:</color> " + _ntfKills;


        private void CountAndRespawnKills(DiedEventArgs ev)
        {
            ev.Target.ClearInventory();
            if (ev.Killer.Role != ev.Target.Role)
            {
                AddAndUpdateKda(ev.Killer);
                AddAndUpdateKda(ev.Target, false);
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
                    Util.PlayerUtil.GlobalBroadcast(30, "Chaos Wins!", true);
                    Util.RoundUtils.LockRound(false);
                    Util.RoundUtils.ForceEndRound(RoleType.ChaosRifleman);
                }
                else if (_chaosKills < _ntfKills)
                {
                    Util.PlayerUtil.GlobalBroadcast(30, "NTF Wins!", true);
                    Util.RoundUtils.LockRound(false);
                    Util.RoundUtils.ForceEndRound(RoleType.NtfCaptain);
                }
                else
                {
                    Util.PlayerUtil.GlobalBroadcast(30, "Tie?", true);
                    Util.RoundUtils.LockRound(false);
                    Util.RoundUtils.ForceEndRound(RoleType.None);
                }
            }
            else
            {
                Util.PlayerUtil.GlobalBroadcast(60, FormatScore(), true);
                Timing.RunCoroutine(SpawnPlayer(ev.Target));
            }
        }

        private IEnumerator<float> SpawnPlayer(Player player)
        {
            yield return Timing.WaitForSeconds(0.1F);
            
            Timing.WaitUntilDone(_teamHandler.SpawnPlayer(player));
            
            yield return Timing.WaitForSeconds(0.1f);
            foreach (var item in _spawnItems) player.AddItem(item);
            
            player.Ammo.Clear();
            
            Log.Info("Round weapon:" + _roundWeapon);

            Timing.WaitForSeconds(.3f);

            var playerWeapon = (Firearm) Item.Create(_roundWeapon, player);
            playerWeapon.Base.ApplyAttachmentsCode(_attachmentCode, false);
            
            Log.Info(playerWeapon.ToString());

            player.AddItem(playerWeapon);
            
            Log.Info($"Waep ammo {playerWeapon.AmmoType}");
            player.Ammo[playerWeapon.AmmoType.GetItemType()] = 120;
            
            player.Inventory.ServerSelectItem(playerWeapon.Serial);
            
            player.IsGodModeEnabled = true;
            yield return Timing.WaitForSeconds(5);
            player.IsGodModeEnabled = false;
        }

        private void AddAndUpdateKda(Player user, bool isKiller = true)
        {
            if (!_kda.ContainsKey(user.Id))
            {
                _kda.Add(user.Id, isKiller ? new KdaHolder(1, 0, user.RankName) : new KdaHolder(0,1, user.RankName));
            }
            else
            {
                if (isKiller)
                    _kda[user.Id].AddKill();
                else
                    _kda[user.Id].AddDeath();
            }
            
            if (!EventfulLab.Instance.Config.EnableTagModifications) return;
            
            user.RankName = _kda[user.Id].ToString();
            user.UpdateRankColorToRole();
        }
    }
    
    internal class KdaHolder
    {
        private int Kill { get; set; }
        private int Death { get; set; }
        private string RankText { get; }

        public KdaHolder(int kill, int death, string rankText)
        {
            Kill = kill;
            Death = death;
            RankText = rankText;
        }

        public void AddKill(int amount = 1) => Kill += amount;

        public void AddDeath(int amount = 1) => Death += amount;

        public override string ToString() => $"/{Kill}/{Death} ${RankText}";
    }
}