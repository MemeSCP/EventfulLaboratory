using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using EventfulLaboratory.structs;
using EXILED;
using EXILED.Extensions;
using MEC;
using UnityEngine;

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
            Common.DisableLightElevators();
            Common.ToggleLockEntranceGate();
            foreach (ReferenceHub player in Player.GetHubs())
            {
                Timing.RunCoroutine(SpawnHubAsParameter(player,
                    player.GetPlayerId() % 2 == 1 ? RoleType.ChaosInsurgency : RoleType.NtfLieutenant));
            }
            Events.PlayerHurtEvent += OnPlayerHurtProxy;
            Events.PlayerDeathEvent += OnPlayerDeathProxy;
            Events.PlayerHandcuffFreedEvent += OnPlayerUncuffed;
            Events.TeamRespawnEvent += Common.PreventRespawnEvent;
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
            Events.PlayerHurtEvent -= OnPlayerHurtProxy;
            Events.PlayerDeathEvent -= OnPlayerDeathProxy;
            Events.PlayerHandcuffFreedEvent -= OnPlayerUncuffed;
            Events.TeamRespawnEvent -= Common.PreventRespawnEvent;
        }

        public override void Reload()
        {
            
        }
        
        private IEnumerator<float> SpawnHubAsParameter(ReferenceHub player, RoleType role)
        {
            yield return Timing.WaitForSeconds(0.3f);
            player.SetRole(role);
            yield return Timing.WaitForSeconds(0.3f);
            player.ClearInventory();
            yield return Timing.WaitForSeconds(0.1f);
            player.AddItem(ItemType.GunUSP);
            
            player.ammoBox.SetOneAmount(0, "30000");
            player.ammoBox.SetOneAmount(1, "30000");
            player.ammoBox.SetOneAmount(2, "30000");
            player.SetMaxHealth(9000);
            player.SetHealth(9000);
        }

        private void OnPlayerHurtProxy(ref PlayerHurtEvent ev) => Timing.RunCoroutine(OnPlayerHurt(ev));

        private IEnumerator<float> OnPlayerHurt(PlayerHurtEvent ev)
        {
            RoleType role = ev.Player.GetRole();
            if (role == RoleType.ChaosInsurgency || role == RoleType.NtfLieutenant)
            {
                RoleType targetRole = role == RoleType.ChaosInsurgency ? RoleType.ClassD : RoleType.Scientist;
                Vector3 loc = ev.Player.GetPosition();
                ev.Player.SetRole(targetRole);
                yield return Timing.WaitForSeconds(0.3f);
                ev.Player.SetPosition(loc);
                ev.Player.effectsController.EnableEffect(Constant.THAWED_EFFECT_API_NAME);
                ev.Player.HandcuffPlayer(ev.Attacker);
                string color1 = ev.Player.GetRole() == RoleType.ChaosInsurgency ? "green" : "blue";
                string color2 = ev.Attacker.GetRole() == RoleType.ChaosInsurgency ? "blue" : "green";
                Common.Broadcast(5, $"<color={color1}>{ev.Player.GetNickname()}</color> has been thawed by <color=${color2}>{ev.Attacker.GetNickname()}</color>!", true);
            } 
            else 
            {
                ev.Player.SetHealth(9000);
            }

            bool isChaos = false, isNtf = false;
            foreach (ReferenceHub player in Player.GetHubs())
            {
                if (!isChaos && player.GetRole() == RoleType.ChaosInsurgency) isChaos = true;
                if (!isNtf && player.GetRole() == RoleType.NtfLieutenant) isNtf = true;
                if (isChaos && isNtf)
                    break;
                else
                {
                    Map.RoundLock = false;
                    if (isChaos)
                    {
                        Common.ForceRoundEnd(RoundSummary.LeadingTeam.ChaosInsurgency);
                    }
                    else if (isNtf)
                    {
                        Common.ForceRoundEnd(RoundSummary.LeadingTeam.FacilityForces);
                    }
                    else
                    {
                        Common.ForceRoundEnd(RoundSummary.LeadingTeam.Draw);
                    }
                    Common.Broadcast(5, "Game End.\nThanks for playing!", true);
                }
            }
        }

        private void OnPlayerDeathProxy(ref PlayerDeathEvent ev) => Timing.RunCoroutine(OnPlayerDeath(ev));

        private IEnumerator<float> OnPlayerDeath(PlayerDeathEvent ev) => RandomPlayerRespawn(ev.Player);

        private void OnPlayerUncuffed(ref HandcuffEvent ev)
        {
            RoleType playerRole = ev.Player.GetRole();
            RoleType targetRole = ev.Target.GetRole();
            if (
                playerRole == RoleType.Scientist || //IF the Player who tried to uncuff is a Scientist (thawed)
                playerRole == RoleType.ClassD ||  // If the Player who tried to uncuff is a ClassD (thawed)
                (playerRole == RoleType.NtfLieutenant && targetRole == RoleType.ClassD) || //If the Player who uncuff is NTF and the target is a ClassD
                (playerRole == RoleType.ChaosInsurgency && targetRole == RoleType.Scientist) //If the Player who uncuff is Chaos and the target is Scientist
            )
            {
                ev.Allow = false;
            }
            else
            {
                Timing.RunCoroutine(RandomPlayerRespawn(ev.Target));
            }
        }

        private IEnumerator<float> RandomPlayerRespawn(ReferenceHub player)
        {
            SpawnHubAsParameter(player,
                player.GetPlayerId() % 2 == 1 ? RoleType.ChaosInsurgency : RoleType.NtfLieutenant);
            yield return Timing.WaitForSeconds(1f);
            player.SetPosition(Common.GetRandomHeavyRoom().Position + new Vector3(0, 4, 0));
        }
    }
}