using System;
using System.Collections.Generic;
using System.Linq;
using Dissonance;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using JetBrains.Annotations;
using Respawning;

namespace EventfulLaboratory
{
    public class Common
    {
        //TODO: Make it per-round getsetter
        [CanBeNull]
        public static Room GetEvacuationZone() => GetRoomByName(Constant.SHELTER_NAME);
        
        public static Room GetRoomByName(String roomName) =>
            Map.Rooms.First(room => room.Name == roomName);

        public static void Broadcast(ushort timing, string text, bool force = false)
        {
            foreach (Player hub in Player.List)
            {
                if (force) hub.ClearBroadcasts();
                hub.Broadcast(timing, text);
            }
        }
        
        public static void LockAllDoors()
        {
            foreach (Door door in Map.Doors)
            {
                door.lockdown = true;
                door.UpdateLock();
            }
        }

        public static void DisableElevators()
        {
            foreach (Lift lift in UnityEngine.Object.FindObjectsOfType<Lift>())
            {
                lift.Lock();
            }
        }

        public static void ToggleLockEntranceGate(bool lockit = true)
        {
            Door gate = Map.Doors.First(door => door.DoorName == Constant.ECZ_GATE);
            if (gate != null)
            {
                gate.locked = lockit;
                gate.UpdateLock();
            }
        }

        public static void LockRound(bool isLocked = true)
        {
            Round.IsLocked = false;
        }
        
        public static void PreventRespawnEvent(RespawningTeamEventArgs ev)
        {
            ev.NextKnownTeam = SpawnableTeamType.None;
        }

        public static Room GetRandomHeavyRoom()
        {
            List<Room> hczRooms = Map.Rooms.Where(room => room.Name.Contains("HCZ")).ToList();
            return hczRooms[new Random().Next(hczRooms.Count -1)];
        }

        public static void ForceEndRound(RoleType winner)
        {
            Team team = Exiled.API.Extensions.Role.GetTeam(winner);
            RoundSummary.escaped_ds = team == Team.CHI ? 1 : 0;
            RoundSummary.escaped_scientists = team == Team.MTF ? 1 : 0;
            RoundSummary.kills_by_scp = team == Team.SCP ? 1 : 0;
            RoundSummary.singleton.ForceEnd();
        }
    }
}