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
            Common.LockRound();
            Common.DisableLightElevators();
            Common.ToggleLockEntranceGate();
            foreach (ReferenceHub player in Player.GetHubs())
            {
                Timing.RunCoroutine(SpawnHubAsParameter(player,
                    player.GetPlayerId() % 2 == 1 ? RoleType.ChaosInsurgency : RoleType.NtfLieutenant));
            }
            Events.PlayerHurtEvent += OnPlayerHurtProxy;
        }

        public override void OnRoundEnd()
        {
            throw new System.NotImplementedException();
        }

        public override void Enable()
        {
            throw new System.NotImplementedException();
        }

        public override void Disable()
        {
            throw new System.NotImplementedException();
        }

        public override void Reload()
        {
            throw new System.NotImplementedException();
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
            } 
            else 
            {
                ev.Player.SetHealth(100);
            }
        }
    }
}