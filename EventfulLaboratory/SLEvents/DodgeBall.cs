using System.Collections.Generic;
using EventfulLaboratory.structs;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
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
            Exiled.Events.Handlers.Player.Joined += PreRoundJoin;
            if (_shelter != null)
            {
                foreach (Player hub in Player.List)
                {
                    Timing.RunCoroutine(MovePlayerToShelter(hub));
                }
            }

            //Timing.RunCoroutine(FewSecToRound());
        }
        
        private void PreRoundJoin(JoinedEventArgs ev)
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

        private IEnumerator<float> MovePlayerToShelter(Player player)
        {
            yield return Timing.WaitForSeconds(1.5f);
            player.SetRole(RoleType.ClassD);
            yield return Timing.WaitForSeconds(0.5f);
            player.Position = _shelter.Position + new Vector3(0, 2, 0);
        }
    }
}