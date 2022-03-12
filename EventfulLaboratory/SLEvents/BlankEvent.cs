using EventfulLaboratory.structs;

namespace EventfulLaboratory.SLEvents
{
    /**
     * Blank Event for the system to munch on when there is no event going on
     */
    public class BlankEvent : AEvent
    {
        public override void OnNewRound()
        {
            //NOOP
        }

        public override void OnRoundStart()
        {
            //NOOP
        }

        public override void OnRoundEnd()
        {
            //NOOP
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
    }
}