using System;
using Dissonance;
using EXILED;
using EXILED.ApiObjects;
using EXILED.Extensions;
using JetBrains.Annotations;

namespace EventfulLaboratory
{
    public class Common
    {
        //TODO: Make it per-round getsetter
        [CanBeNull]
        public static Room GetEvacuationZone() => GetRoomByName(Constant.SHELTER_NAME);
        
        public static Room GetRoomByName(String roomName) =>
            Map.Rooms.Find(room => room.Name == roomName);

        public static void Broadcast(uint timing, string text, bool force = false)
        {
            foreach (ReferenceHub hub in Player.GetHubs())
            {
                if (force) hub.ClearBroadcasts();
                hub.Broadcast(timing, text, false);
            }
        }
        
        public static void LockAllDoors() => 
            Map.Doors.ForEach(door =>
            {
                door.lockdown = true;
                door.UpdateLock();
            });
        
        public static void DisableLightElevators()
        {
            foreach (Lift lift in UnityEngine.Object.FindObjectsOfType<Lift>())
            {
                lift.Lock();
            }
        }

        public static void ForceRoundEnd(RoundSummary.LeadingTeam team)
        {
            bool force = true, allow = true, something = false;
            EXILED.Events.InvokeCheckRoundEnd(ref force, ref allow, ref team, ref something);
        }
        
        public static void LockRound()
        {
            Map.RoundLock = true;
        }
        
        public static void PreventRespawnEvent(ref TeamRespawnEvent ev)
        {
            ev.MaxRespawnAmt = 0;
            ev.ToRespawn.Clear();
        }
    }
}