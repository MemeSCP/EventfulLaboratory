using System.Linq;
using System.Text;
using EventfulLaboratory.structs;
using EXILED;
using EXILED.Patches;

namespace EventfulLaboratory.slevents
{
    public class DebugEvent : AEvent
    {
        public override void OnNewRound()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Rooms: ");
            EXILED.Extensions.Map.Rooms.ForEach(room =>
            {
                builder.Append(room.Name).Append("-").Append(room.Position).Append(", ");
            });
            Logger.Info(builder.ToString());
            Logger.Info("NewRound");
        }

        public override void OnRoundStart()
        {
            Logger.Info("RoundStarted");
        }

        private void OnElevatorInterract(ElevatorInteractionEvent ev)
        {
        }

        public override void OnRoundEnd()
        {
            Logger.Info("RoundEnd");
        }

        public override void Enable()
        {
            Logger.Info("Enabled");
        }

        public override void Disable()
        {
            Logger.Info("Disabled");
        }

        public override void Reload()
        {
            Logger.Info("Reload");
        }
    }
}