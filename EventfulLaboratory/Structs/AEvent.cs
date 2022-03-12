using System;
using System.Linq;
using System.Text;

using EventfulLaboratory.SLEvents;

using Exiled.API.Features;

using HarmonyLib;

namespace EventfulLaboratory.structs
{
    public abstract class AEvent
    {
        public static AEvent GetEvent(LabEvents labEvents)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
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
                case LabEvents.AllRandom:
                    return new AllRandom();
                
                default:
                    if (EventfulLab.Instance.Config.DebugMode)
                    {
                        //Debug only events. Not complete
                        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                        switch (labEvents)
                        {
                            case LabEvents.AmazingRace:
                                return new AmazingRace();
                            case LabEvents.TPDoors:
                                return new TeleportingDoors();
                            case LabEvents.DodgeBall:
                                return new DodgeBall();
                            case LabEvents.HideNSeek:
                                return new HideNSeek();
                            case LabEvents.SuffocationRoom:
                                return new SuffocationRoom();
                            default:
                                return new DebugEvent();
                        }
                    }
                    else
                        return new BlankEvent();
            }
        }

        public static AEvent GetRandomEvent()
        {
            return GetEvent((LabEvents) Util.Random.Next(1, Enum.GetValues(typeof(LabEvents)).Length));
        }

        public string GetName()
        {
            return GetType().Name.Split('.').Last();
        }

        public virtual void OnNewRound()
        {
            var builder = new StringBuilder();
            builder.Append("Rooms: ");

            Room.List.Do(room => { builder.Append(room.Name).Append("-").Append(room.Position.ToString()).Append(", "); });
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