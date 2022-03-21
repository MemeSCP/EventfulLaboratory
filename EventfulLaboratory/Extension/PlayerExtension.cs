using System.Linq;
using CustomPlayerEffects;
using Exiled.API.Extensions;
using Exiled.API.Features;
using UnityEngine;

namespace EventfulLaboratory.Extension
{
    public static class PlayerExtension
    {
        public static void PreventWalking(this Player player)
        {
            player.EnableEffect<Ensnared>();
        }
        
        public static void SetAlmostInvincible(this Player player)
        {
            player.Health = 9001;
            player.AddAhp(
                9001,
                9001,
                0f,
                1f,
                0,
                true
            );
        }

        public static void RestoreWalking(this Player player)
        {
            player.DisableEffect<Ensnared>();
            player.ChangeRunningSpeed(1F, false);
            player.ChangeWalkingSpeed(1.2F, false);
        }

        public static bool IsChaosOrMTF(this Player player) =>
            player.Role.Team == Team.CHI || player.Role.Team == Team.MTF;


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