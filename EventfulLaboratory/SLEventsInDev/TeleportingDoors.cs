using System;
using System.Collections.Generic;
using System.Linq;
using EventfulLaboratory.Extension;
using EventfulLaboratory.structs;
using Exiled.API.Features;
using Interactables.Interobjects.DoorUtils;
using UnityEngine;
using Random = System.Random;

namespace EventfulLaboratory.SLEvents
{
    public class TeleportingDoors : AEvent
    {
        private Dictionary<Door, Door> _doorCache;
        private readonly Random _rng = new Random();

        public override void OnRoundStart()
        {
            _doorCache = Door.List
                .Zip(
                    Door.List.OrderBy(e => _rng.Next()), //Zip Doors to Random Order
                    (from, to) => new {from, to} //Map them to key,value pairs
                )
                .ToDictionary(x => x.from, y => y.to);

            //TODO Temp solution, would work?
            foreach (var door in Door.List)
            {
                Map.PlaceTantrum(door.Position).transform.localScale = new Vector3(.2f, .2f);
            }

            Exiled.Events.Handlers.Player.WalkingOnTantrum += ev =>
            {
                if (ev.Player.Role == RoleType.Spectator) return;

                foreach (var doorVariant in _doorCache)
                {
                    //The door is Closed.
                    if (doorVariant.Key == null || !doorVariant.Key.IsOpen) continue;

                    if (Math.Abs(doorVariant.Key.Position.y - ev.Player.Position.y) > 2) continue;
                    if (Math.Abs(doorVariant.Key.Position.x - ev.Player.Position.x) > 0.5) continue;
                    if (Math.Abs(doorVariant.Key.Position.z - ev.Player.Position.z) > 0.5) continue;

                    var pos = doorVariant.Value.Base.transform;

                    ev.Player.Position = pos.position + pos.forward.ScaleStatic(new Vector3(1.2f, 1.2f, 1.2f)) +
                                         new Vector3(0, 1, 0);

                    return;
                }
            };
                
            // TODO: Maybe patch ourselves
            // Exiled.Events.Handlers.Player.SyncingData

            Exiled.Events.Handlers.Map.Decontaminating += ev => ev.IsAllowed = false;
            foreach (var player in Player.List)
            {
                player.IsBypassModeEnabled = true;
                player.Broadcast(3, "Get out of the facility. Doors are dangerous");
            }
        }
    }
}