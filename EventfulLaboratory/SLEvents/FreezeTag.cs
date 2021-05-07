using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using EventfulLaboratory.Extension;
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
        

        private Dictionary<int, RoleType> _userIdToRole;
        public override void OnNewRound()
        {
            //NOOP
        }

        public override void OnRoundStart()
        {
            _userIdToRole = new Dictionary<int, RoleType>();
            Common.Broadcast(20, "<size=20>Welcome to FreezeTag!\nYour goal is to shoot the <color=red>enemy</color> team, who will transform into their <color=blue>thawed</color> state.\nIF everyone is <color=blue>thawed</color> in the enemy team, your team wins!\nGood luck.\n<color=green>Chaos Insurgency</color> becomes <color=yellow>Scientist></color>\n<color=blue>NTF</color> becomes <color=orange>ClassD</color>.\nUnthawing works by cuffing the correct user!</size>");
            Common.LockRound();
            Common.DisableElevators();
            Common.ToggleLockEntranceGate();
            Common.ToggleTeslats();
            
            foreach (Player player in Player.List)
            {
                Timing.RunCoroutine(RandomPlayerRespawn(player));
            }

            foreach (Room room in Map.Rooms.Where(room =>
                room.Name.Contains("HCZ") &&
                !room.Name.Contains("Tesla") &&
                !room.Name.Contains("EZ_Checkpoint") &&
                !room.Name.Contains("049")
            ).ToList())
            {
                foreach (var doorVariant in room.Doors)
                {
                    if (!doorVariant.TargetState)
                        doorVariant.NetworkTargetState = true;
                }
            }
            Exiled.Events.Handlers.Player.Hurting += OnPlayerHurtProxy;
            //Exiled.Events.Handlers.Player.Died += OnPlayerDeathProxy;
            Exiled.Events.Handlers.Player.Handcuffing += OnCuffEvent;
            Exiled.Events.Handlers.Server.RespawningTeam += Common.PreventRespawnEvent;
            Exiled.Events.Handlers.Player.Spawning += ev => Timing.RunCoroutine(SpawnHubAsParameter(ev.Player,
                ev.Player.Id % 2 == 1 ? _chaosRole : _ntfRole));
        }

        public override void OnRoundEnd()
        {
            Disable();
        }

        public override void Enable()
        {
            
        }

        public override void Disable()
        {
            Exiled.Events.Handlers.Player.Hurting -= OnPlayerHurtProxy;
            //Exiled.Events.Handlers.Player.Died -= OnPlayerDeathProxy;
            Exiled.Events.Handlers.Player.Handcuffing -= OnCuffEvent;
            Exiled.Events.Handlers.Server.RespawningTeam -= Common.PreventRespawnEvent;
        }

        public override void Reload()
        {
            
        }
        
        private IEnumerator<float> SpawnHubAsParameter(Player player, RoleType role)
        {
            yield return Timing.WaitForSeconds(0.3f);
            player.SetRole(role, true);
            yield return Timing.WaitForSeconds(0.1f);
            player.AddItem(ItemType.GunUSP);
            player.AddItem(ItemType.Disarmer);
            player.Inventory.SetCurItem(ItemType.GunUSP);
            player.Inventory.items.ModifyDuration(0, 300);
            
            player.SetAlmostInvincible();
        }

        private void OnPlayerHurtProxy(HurtingEventArgs ev) => Timing.RunCoroutine(OnPlayerHurt(ev));

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
                Common.LockRound(false);
                Common.Broadcast(5, "Game End.\nThanks for playing!\nWinner: " + (toRole), true);
                foreach (Player player in Player.List)
                {
                    player.SetRole(toRole);
                }
                yield return Timing.WaitForSeconds(2F);
                Common.ForceEndRound(toRole);
            }

            ev.IsAllowed = false;
        }

        /*private void OnPlayerDeathProxy(DiedEventArgs ev) => Timing.RunCoroutine(OnPlayerDeath(ev));

        private IEnumerator<float> OnPlayerDeath(DiedEventArgs ev)
        {
            return 
                ev.Target.IsChaosOrMTF() ? 
                    SpawnPlayerAsThawed(ev.Target) : 
                    RandomPlayerRespawn(ev.Target);
        }*/
        
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

        private IEnumerator<float> RandomPlayerRespawn(Player player)
        {
            yield return Timing.WaitUntilDone(RandomPlayerSpawn(player, GetRoleType(player)));
        }

        private IEnumerator<float> RandomPlayerSpawn(Player player, RoleType role)
        {
            
            player.Items.Clear();
            
            yield return Timing.WaitUntilDone(SpawnHubAsParameter(player, role));
            yield return Timing.WaitForSeconds(1f);
            player.RestoreWalking();
            player.Position = Common.GetRandomHeavyRoom().Position + new Vector3(0, 4, 0);
        }

        private IEnumerator<float> SpawnPlayerAsThawed(Player player)
        {
            RoleType targetRole = DetermineThawedRole(GetRoleType(player));
            Vector3 loc = player.Position;
            
            player.Items.Clear();
            
            player.SetRole(targetRole);
            yield return Timing.WaitForSeconds(1f);
            
            player.Items.Clear();
            
            player.Position = loc;
                
            player.SetAlmostInvincible();
            player.PreventWalking();

            player.CufferId = -2;
        }

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

        private RoleType DetermineNextSpawn()
        {
            int chaos = 0, ntf = 0;
            foreach (var pair1 in _userIdToRole)
            {
                if (pair1.Value == _chaosRole) chaos++;
                if (pair1.Value == _ntfRole) ntf++;
            }

            return (chaos > ntf) ? _chaosRole : _ntfRole;
        }

        private RoleType GetRoleType(Player player)
        {
            if (!_userIdToRole.ContainsKey(player.Id))
            {
                _userIdToRole[player.Id] = DetermineNextSpawn();
            }

            return _userIdToRole[player.Id];
        }

        private RoleType DetermineThawedRole(RoleType role)
        {
            return role == _chaosRole ? RoleType.Scientist : RoleType.ClassD;
        }
    }
}