using System;
using System.Collections.Generic;
using System.Linq;
using Dissonance;
using EventfulLaboratory.Extension;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using JetBrains.Annotations;
using Mirror;
using Respawning;
using UnityEngine;
using YamlDotNet.Core;
using Object = System.Object;
using Random = System.Random;

namespace EventfulLaboratory
{
    public class Common
    {
        private static Random rng = new Random();

        //TODO: Make it per-round getsetter
        [CanBeNull]
        public static Room GetEvacuationZone() => GetRoomByName(Constant.SHELTER_NAME);

        public static Room GetRoomByName(string roomName) =>
            Map.Rooms.First(room => room.Name == roomName);

        public static void Broadcast(ushort timing, string text, bool force = false)
        {
            foreach (Player hub in Player.List)
            {
                if (force) hub.ClearBroadcasts();
                //hub.ShowHint(text, timing);
                hub.Broadcast(timing, text);
            }
        }

        public static void Hint(ushort timing, string text, bool force = false)
        {
            foreach (Player hub in Player.List)
            {
                if (force) hub.ClearBroadcasts();
                hub.ShowHint(text, timing);
            }
        }

        public static void LockAllDoors()
        {
            foreach (DoorVariant door in Map.Doors)
            {
                door.ServerChangeLock(DoorLockReason.AdminCommand, true);
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
            foreach (DoorVariant door in Map.Doors)
            {
                if (door.name == Constant.ECZ_GATE)
                {
                    door.ServerChangeLock(DoorLockReason.AdminCommand, lockit);
                }
            }
        }

        public static void LockRound(bool isLocked = true)
        {
            Round.IsLocked = isLocked;
        }

        public static void PreventRespawnEvent(RespawningTeamEventArgs ev)
        {
            ev.NextKnownTeam = SpawnableTeamType.None;
        }

        public static Room GetRandomHeavyRoom()
        {
            List<Room> hczRooms = Map.Rooms.Where(room =>
                room.Name.Contains("HCZ") &&
                !room.Name.Contains("Tesla") &&
                !room.Name.Contains("EZ_Checkpoint") &&
                !room.Name.Contains("049")
            ).ToList();
            return hczRooms[rng.Next(hczRooms.Count - 1)];
        }

        public static DoorVariant GetRandomDoor() => Map.Doors[rng.Next(Map.Doors.Count - 1)];

        public static void ForceEndRound(RoleType winner)
        {
            Team team = Exiled.API.Extensions.Role.GetTeam(winner);
            LockRound(false);

            RoundSummary.escaped_ds = team == Team.CHI ? 1 : 0;
            RoundSummary.escaped_scientists = team == Team.MTF ? 1 : 0;
            RoundSummary.kills_by_scp = team == Team.SCP ? 1 : 0;
            foreach (var player in Player.List)
            {
                player.SetRole(winner);
            }

            RoundSummary.singleton.ForceEnd();
        }

        public static void ToggleTeslats(Boolean state = false)
        {
            foreach (var teslaGate in Map.TeslaGates)
            {
                teslaGate.enabled = state;
            }
        }

        public static GameObject JustFuckingSpawnADoor(
            Vector3 pos,
            Vector3? pRot = null,
            Vector3? pScale = null,
            bool isOpenable = false,
            bool isAlmostIndestructable = true
        )
        {
            Vector3 rotation = pRot ?? new Vector3(0, 0, 0);
            Vector3 scale = pScale ?? new Vector3(1f, 1f, 1f);

            var prefab = NetworkManager.singleton.spawnPrefabs.Find(p => p.gameObject.name == "HCZ BreakableDoor");
            //https://www.youtube.com/watch?v=ZLiN2Js1UtQ
            var door = UnityEngine.Object.Instantiate(prefab);

            door.transform.localPosition = pos;
            door.transform.localRotation = Quaternion.Euler(rotation);
            door.transform.localScale = scale;

            var doorVariant = door.GetComponent<BreakableDoor>();

            if (!isOpenable) doorVariant.ServerChangeLock(DoorLockReason.AdminCommand, true);
            if (isAlmostIndestructable) doorVariant.ServerDamage(float.MinValue + 80f, DoorDamageType.ServerCommand);

            //door.UpdatePlayers();

            NetworkServer.Spawn(door);
            return door;
        }

        public static GameObject SpawnPrefab(
            string prefabName,
            Vector3 pos,
            Vector3? pRot = null,
            Vector3? pScale = null
        )
        {
            return SpawnPrefab(NetworkManager.singleton.spawnPrefabs.Find(p => p.gameObject.name == prefabName),pos, pRot, pScale);
        }
        
        

        public static GameObject SpawnPrefab(
            GameObject prefab,
            Vector3 pos,
            Vector3? pRot = null,
            Vector3? pScale = null
        )
        {
            Vector3 rotation = pRot ?? new Vector3(0, 0, 0);
            Vector3 scale = pScale ?? new Vector3(1f, 1f, 1f);
            
            var gameO = UnityEngine.Object.Instantiate(prefab);

            gameO.transform.localPosition = pos;
            gameO.transform.localRotation = Quaternion.Euler(rotation);
            gameO.transform.localScale = scale;

            gameO.NotifyPlayers();

            NetworkServer.Spawn(gameO);
            return gameO;
        }

        public static Random GetRandom() => rng;
        
        public static GameObject HandleSpawning(string name, Vector3 pos, Quaternion rot, Vector3 scl)
        {
            switch (name)
            {
                case "HCZ BreakableDoor(Clone)":
                case "HCZ BreakableDoor":
                    return JustFuckingSpawnADoor(
                        pos,
                        rot.eulerAngles,
                        scl
                    );
                default:
                    if (!name.StartsWith("B272sa")) return SpawnPrefab(name, pos, rot.eulerAngles, scl);
                    
                    
                    foreach (NetworkIdentity ni in UnityEngine.Object.FindObjectsOfType<NetworkIdentity>())
                    {
                        if (ni.gameObject.name == name)
                        {
                            return SpawnPrefab(ni.gameObject, pos, rot.eulerAngles, scl);
                        }
                    }
                    return SpawnPrefab(name, pos, rot.eulerAngles, scl);
            }
            return null;
        }
    }

}