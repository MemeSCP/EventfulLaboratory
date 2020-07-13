using System;
using System.Linq;
using EventfulLaboratory.slevents;
using EXILED.Patches;
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
                    if (Plugin.DEBUG)
                        return new DebugEvent();
                    else
                        return new BlankEvent();
            }
        }

        public static AEvent GetRandomEvent()
        {
            return GetEvent((LabEvents)new Random().NextInt(1, 4));
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