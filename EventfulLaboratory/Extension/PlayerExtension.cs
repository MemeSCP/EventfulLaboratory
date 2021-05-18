using System;
using System.Linq;
using Exiled.API.Extensions;
using Exiled.API.Features;
using UnityEngine;

namespace EventfulLaboratory.Extension
{
    public static class PlayerExtension
    {
        public static void PreventWalking(this Player player)
        {
            player.ChangeRunningSpeed(0F, false);
            player.ChangeWalkingSpeed(0F, false);
        }
        
        public static void SetAlmostInvincible(this Player player)
        {
            player.Health = 9001;
            player.ArtificialHealth = 9001;
            player.ArtificialHealthDecay = 0;
        }

        public static void RestoreWalking(this Player player)
        {
            player.ChangeRunningSpeed(1F, false);
            player.ChangeWalkingSpeed(1.2F, false);
        }

        public static Boolean IsChaosOrMTF(this Player player) =>
            player.Role.GetTeam() == Team.CHI || player.Role.GetTeam() == Team.MTF;


        public static void UpdateRankColorToRole(this Player player) => player.UpdateRankColorToRole(player.Role);
        
        public static void UpdateRankColorToRole(this Player player, RoleType role)
        {
            player.RankColor = player.ReferenceHub.serverRoles.NamedColors.FirstOrDefault(color => !color.Restricted && color.ColorHex == role.GetColor().ToHex())?.Name ?? player.RankColor;
        }

        public static RaycastHit Raytrace(this Player player, Vector3? pOffset = null)
        {
            var rotation = player.Rotation;
            var offset = pOffset ?? Vector3.one.ScaleStatic(rotation);
            var position = player.CameraTransform.position;
            Logger.Info($"Offset:{offset}\nPosition: {player.Position}\nRayStart: {position + offset}\nRotation: {player.Rotation}");
            Physics.Raycast(
                position + offset,
                player.Rotation,
                out var hit
            );
            return hit;
        }
    }
}