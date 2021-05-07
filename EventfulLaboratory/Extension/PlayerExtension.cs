using System;
using Exiled.API.Extensions;
using Exiled.API.Features;

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
    }
}