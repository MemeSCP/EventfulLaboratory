using System.Text;
using EventfulLaboratory.structs;
using Exiled.API.Features;
using HarmonyLib;

namespace EventfulLaboratory.SLEvents
{
    public class DebugEvent : AEvent
    {
        public override void OnNewRound()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Rooms: ");
            
            Room.List.Do(room =>
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