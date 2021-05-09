using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using EventfulLaboratory.Extension;
using EventfulLaboratory.Handler;
using EventfulLaboratory.structs;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events;
using Exiled.Events.EventArgs;
using Interactables.Interobjects.DoorUtils;
using MEC;
using UnityEngine;

namespace EventfulLaboratory.slevents
{
    public class FreezeTag : AEvent
    {
        //TODO: Players into hashMap

        private readonly RoleType _ntfRole = RoleType.NtfLieutenant;
        private readonly RoleType _chaosRole = RoleType.ChaosInsurgency;

        private EvenTeamSplitHandler _teamHandler;

        public override void OnRoundStart()
        {
            _teamHandler = new EvenTeamSplitHandler(_ntfRole, _chaosRole, false);
            
            Common.Broadcast(20, "<size=20>Welcome to FreezeTag!\nYour goal is to shoot the <color=red>enemy</color> team, who will transform into their <color=blue>thawed</color> state.\nIF everyone is <color=blue>thawed</color> in the enemy team, your team wins!\nGood luck.\n<color=green>Chaos Insurgency</color> becomes <color=yellow>Scientist></color>\n<color=blue>NTF</color> becomes <color=orange>ClassD</color>.\nUnthawing works by cuffing the correct user!</size>");
            Common.LockRound();
            Common.DisableElevators();
            Common.ToggleLockEntranceGate();
            Common.ToggleTeslats();
            
            foreach (var player in Player.List)
            {
                Timing.RunCoroutine(RandomPlayerRespawn(player));
            }

            foreach (var room in GetHeavyRoomsBlocklisted())
            {
                foreach (var doorVariant in room.Doors)
                {
                    if (!doorVariant.TargetState)
                        doorVariant.NetworkTargetState = true;
                }
            }
            Exiled.Events.Handlers.Player.Hurting += OnPlayerHurtProxy;
            Exiled.Events.Handlers.Player.Handcuffing += OnCuffEvent;
            Exiled.Events.Handlers.Server.RespawningTeam += Common.PreventRespawnEvent;
            Exiled.Events.Handlers.Player.Joined += OnPlayerJoined;
            Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
        }

        public override void OnRoundEnd()
        {
            Disable();
        }

        public override void Disable()
        {
            Exiled.Events.Handlers.Player.Hurting -= OnPlayerHurtProxy;
            Exiled.Events.Handlers.Player.Handcuffing -= OnCuffEvent;
            Exiled.Events.Handlers.Server.RespawningTeam -= Common.PreventRespawnEvent;
            Exiled.Events.Handlers.Player.Joined -= OnPlayerJoined;
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeft;
        }

        private RoleType DetermineThawedRole(RoleType role)
        {
            return role == _chaosRole ? RoleType.Scientist : RoleType.ClassD;
        }

        #region Proxies
        
        private void OnPlayerHurtProxy(HurtingEventArgs ev) => Timing.RunCoroutine(OnPlayerHurt(ev));
        
        #endregion
        
        #region Spawning
        
        private IEnumerator<float> RandomPlayerSpawn(Player player)
        {
            player.Items.Clear();
            
            yield return Timing.WaitUntilDone(SpawnHubAsParameter(player));
            yield return Timing.WaitForSeconds(1f);
            player.RestoreWalking();
            player.Position = Common.GetRandomHeavyRoom().Position + new Vector3(0, 4, 0);
        }
        
        private IEnumerator<float> RandomPlayerRespawn(Player player)
        {
            yield return Timing.WaitUntilDone(RandomPlayerSpawn(player));
        }

        private IEnumerator<float> SpawnPlayerAsThawed(Player player)
        {
            RoleType targetRole = DetermineThawedRole(_teamHandler.GetSetRole(player));
            Vector3 loc = player.Position;
            
            player.Items.Clear();
            
            player.SetRole(targetRole);
            yield return Timing.WaitForSeconds(1f);
            
            player.Items.Clear();
            
            player.Position = loc;
                
            player.SetAlmostInvincible();
            player.PreventWalking();
            
            if (EventfulLab.Instance.Config.EnableTagModifications)
                player.UpdateRankColorToRole();
        }
        private IEnumerator<float> SpawnHubAsParameter(Player player)
        {
            RoleType role = _teamHandler.GetSetRole(player);
            yield return Timing.WaitForSeconds(0.3f);
            player.SetRole(role, true);
            yield return Timing.WaitForSeconds(0.1f);
            player.AddItem(ItemType.GunUSP);
            player.AddItem(ItemType.Disarmer);
            player.Inventory.SetCurItem(ItemType.GunUSP);
            player.Inventory.items.ModifyDuration(0, 300);
            player.EnableEffect<Scp207>();
            player.GetEffect(EffectType.Scp207).Intensity = 1;
            
            player.SetAlmostInvincible();

            if (EventfulLab.Instance.Config.EnableTagModifications)
            {
                player.UpdateRankColorToRole(role);
            }
        }
        
        #endregion
        
        #region Events

        private void OnPlayerJoined(JoinedEventArgs ev)
        {
            Timing.RunCoroutine(SpawnHubAsParameter(ev.Player));
        }
        
        private void OnPlayerLeft(LeftEventArgs ev)
        {
            _teamHandler.SetDirty();
        }
        
        private IEnumerator<float> OnPlayerHurt(HurtingEventArgs ev)
        {
            ev.IsAllowed = false;
            ev.Target.SetAlmostInvincible();
            
            if (ev.DamageType != DamageTypes.Usp) yield break;
            
            RoleType role = ev.Target.Role;

            if (role == _chaosRole || role == _ntfRole)
            {
                string color1 = ev.Target.Role == _chaosRole ? "green" : "blue";
                string color2 = ev.Attacker.Role == _chaosRole ? "green" : "blue";

                yield return SpawnPlayerAsThawed(ev.Target).WaitUntilDone();

                AnnounceThawing(ev.Attacker, ev.Target, color1, color2);
            }

            ev.Target.SetAlmostInvincible();

            if (EventfulLab.Instance.Config.DebugMode) yield break;            
            
            bool isChaos = false, isNtf = false;
            foreach (Player player in Player.List)
            {
                
                if (!isChaos && player.Role == _chaosRole)
                {
                    isChaos = true;
                    continue;
                }

                if (!isNtf && player.Role == _ntfRole)
                {
                    isNtf = true;
                    continue;
                }
                
                if (isChaos && isNtf)
                    break;
            }

            if (!isChaos || !isNtf)
            {
                RoleType toRole = (!isChaos && isNtf ? _ntfRole : _chaosRole);
                Common.Broadcast(5, "Game End.\nThanks for playing!\nWinner: " + (toRole), true);
                
                yield return Timing.WaitForSeconds(2F);
                Common.ForceEndRound(toRole);
            }
        }
        
        private void OnCuffEvent(HandcuffingEventArgs ev)
        {
            RoleType source = ev.Cuffer.Role;
            RoleType target = ev.Target.Role;
            if (
                source == _ntfRole && target == RoleType.ClassD ||
                source == _chaosRole && target == RoleType.Scientist
            )
            {
                //Allowed to do cuffing
                Timing.RunCoroutine(RandomPlayerRespawn(ev.Target));
                string color = ev.Cuffer.Role == _chaosRole ? "green" : "blue";
                AnnounceUnthawing(ev.Cuffer, ev.Target, color , color);
            }
            else
            {
                ev.IsAllowed = false;
                ev.Cuffer.ShowHint("You cannot cuff that person! Bad!");
            }
        }
        
        #endregion

        private void AnnounceThawing(Player thawer, Player target, string color1, string color2)
        {
            Common.Broadcast(5,
                $"<color={color1}>{thawer.Nickname}</color> has been thawed by <color={color2}>{target.Nickname}</color>!",
                true);
        }

        private void AnnounceUnthawing(Player thawer, Player target, string color1, string color2)
        {
            Common.Broadcast(5,
                $"<color={color1}>{target.Nickname}</color> has been <size=30>unthawed</size> by <color={color2}>{thawer.Nickname}</color>!",
                true);
        }

        private List<Room> GetHeavyRoomsBlocklisted() => Map.Rooms.Where(room =>
            room.Name.Contains("HCZ") &&
            !room.Name.Contains("Tesla") &&
            !room.Name.Contains("EZ_Checkpoint") &&
            !room.Name.Contains("049")
        ).ToList();
    }
}