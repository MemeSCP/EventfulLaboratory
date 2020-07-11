using System;
using Mirror;
using UnityEngine;

namespace EventfulLaboratory.Effects
{
    public class Thawed : PlayerEffect
    {
        private Vector3 _thawedPos = Vector3.zero;

        public Thawed(PlayerStats ps, String apiName)
        {
            ApiName = apiName;
            Player = ps;
            Slot = ConsumableAndWearableItems.UsableItem.ItemSlot.Unwearable;
        }
        
        public override void ServerOnClassChange(RoleType previousClass, RoleType newClass)
        {
            ServerDisable();
        }

        public override void OnUpdate()
        {
            if (!Enabled || !NetworkServer.active)
                return;
            PlyMovementSync ply = Player.GetComponent<PlyMovementSync>();
            if (ply != null)
            {
                if (_thawedPos == Vector3.zero)
                {
                    _thawedPos = ply.RealModelPosition;
                }
                ply.TargetForcePosition(ply.connectionToServer, _thawedPos);
            }
        }
    }
}