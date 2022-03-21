using System.Collections.Generic;
using EventfulLaboratory.structs;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs;
using MEC;

namespace EventfulLaboratory.SLEvents
{
    internal sealed class EscapeRace : AEvent
    {
        public override void OnRoundStart()
        {
            Exiled.Events.Handlers.Server.RespawningTeam += Util.RoundUtils.PreventRespawnEvent;
            Exiled.Events.Handlers.Player.Joined += PlayerLatejoin;
            foreach (var player in Player.List) Timing.RunCoroutine(SpawnAsClassDOrScientist(player));
            Util.PlayerUtil.GlobalBroadcast(
                15, 
                "<size=15>Welcome to EscapeRace!</size><br/>Your goal is to escape and kill the other team!</br>Everyone has a gun! Be careful!<br/>(No keycards or healthkit at hand! Scavange on!)", true
            );

            Timing.CallDelayed(
                30f,
                () => Exiled.Events.Handlers.Player.Joined -= PlayerLatejoin
            );
        }

        public override void OnRoundEnd() => Exiled.Events.Handlers.Server.RespawningTeam -= Util.RoundUtils.PreventRespawnEvent;

        private void PlayerLatejoin(JoinedEventArgs args) => Timing.RunCoroutine(SpawnAsClassDOrScientist(args.Player));

        private static IEnumerator<float> SpawnAsClassDOrScientist(Player player)
        {
            player.SetRole(player.Id % 2 == 0 ? RoleType.ClassD : RoleType.Scientist);
            yield return Timing.WaitForSeconds(.1f);
            player.ClearInventory();
            yield return Timing.WaitForSeconds(.1f);
            player.AddItem(Item.Create(ItemType.GunRevolver));
            player.AddAmmo(ItemType.GunRevolver.GetAmmoType(), 90);
        }
    }
}