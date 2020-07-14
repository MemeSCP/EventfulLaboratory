using System;
using System.Collections.Generic;
using System.Linq;
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

        public static void ToggleLockEntranceGate(bool lockit = true)
        {
            Door gate = Map.Doors.Find(door => door.DoorName == Constant.ECZ_GATE);
            if (gate != null)
            {
                gate.locked = lockit;
                gate.UpdateLock();
            }
        }

        public static void ForceRoundEnd(RoundSummary.LeadingTeam team)
        {
            
        }

        private static void _ForceRoundEndProxy(ref CheckRoundEndEvent ev)
        {
            
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

        public static Room GetRandomHeavyRoom()
        {
            List<Room> hczRooms = Map.Rooms.FindAll(room => room.Name.Contains("HCZ"));
            return hczRooms[new Random().Next(hczRooms.Count -1)];
        }
    }
}