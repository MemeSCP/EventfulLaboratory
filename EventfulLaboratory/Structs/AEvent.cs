using System;
using System.Linq;
using EventfulLaboratory.slevents;
using EXILED.Patches;

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
                default:
                    if (Plugin.DEBUG)
                        return new DebugEvent();
                    else
                        return new BlankEvent();
            }
        }

        public String GetName()
        {
            return GetType().Name.Split('.').Last();
        }
        
        public abstract void OnNewRound();
        
        public abstract void OnRoundStart();

        public abstract void OnRoundEnd();

        public abstract void Enable();

        public abstract void Disable();

        public abstract void Reload();
    }
}