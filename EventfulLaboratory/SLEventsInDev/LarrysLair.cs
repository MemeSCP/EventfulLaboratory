using System.Collections.Generic;
using System.Linq;
using EventfulLaboratory.structs;
using Exiled.API.Features;
using MEC;

namespace EventfulLaboratory.slevents
{
    public class LarrysLair : AEvent
    {
        private List<Player> _larries;

        
        //region Overrides
        public override void OnNewRound()
        {
            _larries = new List<Player>();
        }

        public override void OnRoundStart() => Timing.RunCoroutine(RoundStartRoutine());

        public override void OnRoundEnd()
        {
            
        }
        
        //endregion
        
        //region Events
        IEnumerator<float> RoundStartRoutine()
        {
            _larries = Util.PlayerUtil.GetRandomPlayers(4).ToList();
            
            foreach (var randomPlayer in Player.List)
            {
                if (_larries.Contains(randomPlayer))
                {
                    randomPlayer.SetRole(RoleType.Scp106);
                }
                else
                {
                    randomPlayer.SetRole(RoleType.ClassD);
                    yield return Timing.WaitForSeconds(0.1f);

                    randomPlayer.AddItem(ItemType.Flashlight);
                }
            }
            
            Util.MapUtil.TurnOffLights();
            
            Util.PlayerUtil.GlobalBroadcast(
                30,
                    "<size=20>" +
                        "<color=red>Welcome to Larry's Lair!</color>\n" +
                        "The facility has taken over by <color=red>multiple</color> 106es.\n" +
                        "Your job is to escape the facility and detonate the nuke.\n" +
                        "Nuke "
                );
        }


        //endregion
    }
}