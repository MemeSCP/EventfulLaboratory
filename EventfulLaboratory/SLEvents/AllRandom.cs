using EventfulLaboratory.structs;
using Exiled.API.Features;
using Exiled.Events.EventArgs;

namespace EventfulLaboratory.SLEvents
{
    public class AllRandom : AEvent
    {
        public override void OnRoundStart()
        {
            Exiled.Events.Handlers.Player.Joined += OnPlayerJoin;
            
            foreach (var player in Player.List)
            {
                SpawnPlayerAsRandom(player);
            }
        }

        public override void Disable() => OnRoundEnd();

        public override void OnRoundEnd()
        {
            Exiled.Events.Handlers.Player.Joined -= OnPlayerJoin;
        }

        private void OnPlayerJoin(JoinedEventArgs ev) => SpawnPlayerAsRandom(ev.Player);

        private void SpawnPlayerAsRandom(Player player)
        {
            RoleType role;
            while (true)
            {
                role = (RoleType) Util.Random.Next(20);
                    
                //Spec and Tutorial is blocked
                if (role == RoleType.Spectator ||
                    role == RoleType.Tutorial) continue;

                break;
            }
                
            player.SetRole(role);
        }
    }
}