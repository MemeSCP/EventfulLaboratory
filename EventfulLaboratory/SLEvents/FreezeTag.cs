using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using CustomPlayerEffects;
using EventfulLaboratory.structs;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events;
using Exiled.Events.EventArgs;
using MEC;
using UnityEngine;
using Round = Exiled.Events.Handlers.Round;

namespace EventfulLaboratory.slevents
{
    public class FreezeTag : AEvent
    {
        public override void OnNewRound()
        {
            //NOOP
        }

        public override void OnRoundStart()
        {
            Common.Broadcast(15, "<size=20>Welcome to FreezeTag!\nYour goal is to shoot the <color=red>enemy</color> team, who will transform into their <color=blue>thawed</color> state.\nIF everyone is <color=blue>thawed</color> in the enemy team, your team wins!\nGood luck.</size>");
            Common.LockRound();
            Common.DisableElevators();
            Common.ToggleLockEntranceGate();
            foreach (Player player in Player.List)
            {
                Timing.RunCoroutine(SpawnHubAsParameter(player,
                    player.Id % 2 == 1 ? RoleType.ChaosInsurgency : RoleType.NtfLieutenant));
            }
            Exiled.Events.Handlers.Player.Hurting += OnPlayerHurtProxy;
            Exiled.Events.Handlers.Player.Died += OnPlayerDeathProxy;
            Exiled.Events.Handlers.Player.RemovingHandcuffs += OnPlayerUncuffed;
            Exiled.Events.Handlers.Server.RespawningTeam += Common.PreventRespawnEvent;
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
            Exiled.Events.Handlers.Player.Died -= OnPlayerDeathProxy;
            Exiled.Events.Handlers.Player.RemovingHandcuffs -= OnPlayerUncuffed;
            Exiled.Events.Handlers.Server.RespawningTeam -= Common.PreventRespawnEvent;
        }

        public override void Reload()
        {
            
        }
        
        private IEnumerator<float> SpawnHubAsParameter(Player player, RoleType role)
        {
            yield return Timing.WaitForSeconds(0.3f);
            player.SetRole(role);
            yield return Timing.WaitForSeconds(0.3f);
            player.ClearInventory();
            yield return Timing.WaitForSeconds(0.1f);
            player.AddItem(ItemType.GunUSP);
            player.Inventory.SetCurItem(ItemType.GunUSP);
            player.Inventory.items.ModifyDuration(0, 300);
            
            player.MaxHealth = 9000;
            player.Health = 9000;
        }

        private void OnPlayerHurtProxy(HurtingEventArgs ev) => Timing.RunCoroutine(OnPlayerHurt(ev));

        private IEnumerator<float> OnPlayerHurt(HurtingEventArgs ev)
        {
            RoleType role = ev.Target.Role;
            if (role == RoleType.ChaosInsurgency || role == RoleType.NtfLieutenant)
            {
                RoleType targetRole = role == RoleType.ChaosInsurgency ? RoleType.ClassD : RoleType.Scientist;
                Vector3 loc = ev.Target.Position;
                ev.Target.SetRole(targetRole);
                yield return Timing.WaitForSeconds(0.3f);
                ev.Target.Position = loc;
                ev.Target.ReferenceHub.playerEffectsController.EnableEffect<Disabled>(100000f, true);
                ev.Target.Handcuff(ev.Attacker);
                string color1 = ev.Target.Role == RoleType.ChaosInsurgency ? "green" : "blue";
                string color2 = ev.Attacker.Role == RoleType.ChaosInsurgency ? "blue" : "green";
                Common.Broadcast(5,
                    $"<color={color1}>{ev.Target.Nickname}</color> has been thawed by <color=${color2}>{ev.Attacker.Nickname}</color>!",
                    true);
            }
            else
            {
                ev.Target.Health = 9000;
            }
            
            bool isChaos = false, isNtf = false;
            foreach (Player player in Player.List)
            {
                if (!isChaos && player.Role == RoleType.ChaosInsurgency) isChaos = true;
                if (!isNtf && player.Role == RoleType.NtfLieutenant) isNtf = true;
                if (isChaos && isNtf)
                    break;
                Common.LockRound(false);
                Common.ForceEndRound(!isChaos && isNtf ? RoleType.NtfLieutenant : RoleType.ChaosInsurgency);
                Common.Broadcast(5, "Game End.\nThanks for playing!", true);
            }
            
            ev.IsAllowed = false;
        }

        private void OnPlayerDeathProxy(DiedEventArgs ev) => Timing.RunCoroutine(OnPlayerDeath(ev));

        private IEnumerator<float> OnPlayerDeath(DiedEventArgs ev) => RandomPlayerRespawn(ev.Target);

        private void OnPlayerUncuffed(RemovingHandcuffsEventArgs ev)
        {
            RoleType playerRole = ev.Cuffer.Role;
            RoleType targetRole = ev.Target.Role;
            if (
                playerRole == RoleType.Scientist || //IF the Player who tried to uncuff is a Scientist (thawed)
                playerRole == RoleType.ClassD ||  // If the Player who tried to uncuff is a ClassD (thawed)
                (playerRole == RoleType.NtfLieutenant && targetRole == RoleType.ClassD) || //If the Player who uncuff is NTF and the target is a ClassD
                (playerRole == RoleType.ChaosInsurgency && targetRole == RoleType.Scientist) //If the Player who uncuff is Chaos and the target is Scientist
            )
            {
                ev.IsAllowed = false;
            }
            else
            {
                Timing.RunCoroutine(RandomPlayerRespawn(ev.Target));
            }
        }

        private IEnumerator<float> RandomPlayerRespawn(Player player)
        {
            SpawnHubAsParameter(player,
                player.Id % 2 == 1 ? RoleType.ChaosInsurgency : RoleType.NtfLieutenant);
            yield return Timing.WaitForSeconds(1f);
            player.Position = Common.GetRandomHeavyRoom().Position + new Vector3(0, 4, 0);
        }
    }
}