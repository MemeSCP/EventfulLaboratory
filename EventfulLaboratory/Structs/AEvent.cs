using System;
using System.Linq;
using System.Text;

using EventfulLaboratory.slevents;

using Exiled.API.Features;

using HarmonyLib;

using Random = Unity.Mathematics.Random;

namespace EventfulLaboratory.structs
{
    public abstract class AEvent
    {
        public static AEvent GetEvent(LabEvents labEvents)
        {
            switch (labEvents)
            {
                case LabEvents.PeanutChamber:
                    return new PeanutChamber();
                case LabEvents.TeamWarfare:
                    return new TeamWarfare();
                case LabEvents.PeanutVirus:
                    return new PeanutVirus();
                case LabEvents.FreezeTag:
                    return new FreezeTag();
                default:
                    if (EventfulLab.Instance.Config.DebugMode)
                        return new DebugEvent();
                    else
                        return new BlankEvent();
            }
        }

        public static AEvent GetRandomEvent()
        {
            return GetEvent((LabEvents) new Random().NextInt(1, Enum.GetValues(typeof(LabEvents)).Length));
        }

        public string GetName()
        {
            return GetType().Name.Split('.').Last();
        }

        public virtual void OnNewRound()
        {
            var builder = new StringBuilder();
            builder.Append("Rooms: ");

            Map.Rooms.Do(room => { builder.Append(room.Name).Append("-").Append(room.Position).Append(", "); });
            Logger.Debug(builder.ToString());
            Logger.Debug("NewRound");
        }

        public virtual void OnRoundStart()
        {
            Logger.Debug("RoundStarted");
        }

        public virtual void OnRoundEnd()
        {
            Logger.Debug("RoundEnd");
        }

        public virtual void Enable()
        {
            Logger.Debug("Enabled");
        }

        public virtual void Disable()
        {
            Logger.Debug("Disabled");
        }

        public virtual void Reload()
        {
            Logger.Debug("Reload");
        }
    }
}