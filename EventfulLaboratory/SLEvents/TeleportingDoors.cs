using System;
using System.Collections.Generic;
using EventfulLaboratory.Extension;
using EventfulLaboratory.structs;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using Interactables.Interobjects.DoorUtils;
using UnityEngine;
using Map = Exiled.Events.Handlers.Map;

namespace EventfulLaboratory.slevents
{
    public class TeleportingDoors : AEvent
    {
        private List<DoorVariant> _doorCache = new List<DoorVariant>();

        private readonly Vector3[] dirs = {
            new Vector3(-1, 1, -1),
            new Vector3(-1, 1, -1),
            new Vector3(1, 1, 1),
            new Vector3(1, 1, 1),
        };

        Vector3 GetDir(DoorVariant door)
        {
            var rot = Math.Round(door.transform.rotation.eulerAngles.y);
            return dirs[(int) (rot / 90)];
        }
        
        public override void OnRoundStart()
        {
            Exiled.Events.Handlers.Player.InteractingDoor += ev =>
            {
                if (!ev.Door.TargetState)
                {
                    if (!_doorCache.Contains(ev.Door))
                        _doorCache.Add(ev.Door);
                }

                Logger.Info(
                    (Math.Abs(ev.Door.transform.position.y - ev.Player.Position.y) > 2) + "/" +
                    (Math.Abs(ev.Door.transform.position.x - ev.Player.Position.x) > 0.5) + "/" +
                    (Math.Abs(ev.Door.transform.position.z - ev.Player.Position.z) > 0.5)
                    );
            };
            
            Exiled.Events.Handlers.Player.SyncingData += ev =>
            {
                if (ev.Player.Role == RoleType.Spectator) return;

                foreach (var doorVariant in _doorCache)
                {
                    //The door is Closed.
                    if (doorVariant == null || !doorVariant.IsConsideredOpen()) continue;

                    if (Math.Abs(doorVariant.transform.position.y - ev.Player.Position.y) > 2) continue;
                    if (Math.Abs(doorVariant.transform.position.x - ev.Player.Position.x) > 0.5) continue;
                    if (Math.Abs(doorVariant.transform.position.z - ev.Player.Position.z) > 0.5) continue;
                    
                    var door = Common.GetRandomDoor();

                    while (door == null) door = Common.GetRandomDoor();
                    
                    var pos = door.transform;

                    ev.Player.Position = pos.position + pos.forward.ScaleStatic(new Vector3(1.2f, 1.2f, 1.2f)) + new Vector3(0, 1 ,0);
                    //ev.Player.Position = pos.position + GetDir(door);
                    
                    return;
                }
            };

            Exiled.Events.Handlers.Map.Decontaminating += ev => ev.IsAllowed = false;
            foreach (var player in Player.List)
            {
                player.IsBypassModeEnabled = true;
                player.Broadcast(3, "Get out of the facility. Doors are dangerous");
            }
        }
    }
}