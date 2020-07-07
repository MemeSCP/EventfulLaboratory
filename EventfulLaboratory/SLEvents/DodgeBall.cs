using System.Collections.Generic;
using EventfulLaboratory.structs;
using EXILED;
using EXILED.ApiObjects;
using EXILED.Extensions;
using MEC;
using UnityEngine;

namespace EventfulLaboratory.slevents
{
    public class DodgeBall: AEvent
    {
        private Room _shelter;
        
        public override void OnNewRound()
        {
            //NOOP
        }

        public override void OnRoundStart()
        {
            Common.LockRound();
            Common.LockAllDoors();
            _shelter = Common.GetEvacuationZone();
            Events.PlayerJoinEvent += PreRoundJoin;
            if (_shelter != null)
            {
                foreach (ReferenceHub hub in Player.GetHubs())
                {
                    Timing.RunCoroutine(MovePlayerToShelter(hub));
                }
            }

            //Timing.RunCoroutine(FewSecToRound());
        }
        
        private void PreRoundJoin(PlayerJoinEvent ev)
        {
            Timing.RunCoroutine(MovePlayerToShelter(ev.Player));
        }

        public override void OnRoundEnd()
        {
            
        }

        public override void Enable()
        {
            //NOOP
        }

        public override void Disable()
        {
            //NOOP
        }

        public override void Reload()
        {
            //NOOP
        }

        private IEnumerator<float> MovePlayerToShelter(ReferenceHub hub)
        {
            yield return Timing.WaitForSeconds(1.5f);
            hub.SetRole(RoleType.ClassD);
            yield return Timing.WaitForSeconds(0.5f);
            hub.SetPosition(_shelter.Position + new Vector3(0, 2, 0));
        }
    }
}