using System.Collections.Generic;
using System.Linq;
using EventfulLaboratory.structs;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using UnityEngine;
using Random = System.Random;

namespace EventfulLaboratory.SLEvents
{
    public class PeanutVirus : AEvent
    {
        private Player _mainPeanut;
        
        public override void OnNewRound()
        {
            Util.RoundUtils.LockRound();

            Exiled.Events.Handlers.Server.RestartingRound += RoundRestartEvent;
            Exiled.Events.Handlers.Server.RespawningTeam += Util.RoundUtils.PreventRespawnEvent;
        }

        public override void OnRoundStart()
        {
            Util.PlayerUtil.GlobalBroadcast(15, "Welcome to Peanut Virus. Avoid SCP 173. On death consequences happen.");
            Timing.RunCoroutine(RoundStartEnumerator());    
        }

        private IEnumerator<float> RoundStartEnumerator()
        {
            yield return Timing.WaitForSeconds(1f);
            List<Player> players = Player.List.ToList();
            _mainPeanut = players[new Random().Next(players.Count)];
            Room peanutStart = Util.MapUtil.GetRoomByName(Constant.FOUR_NINE_CHAMBER);
            Room dStart = Util.MapUtil.GetRoomByName(Constant.SEVEN_NINE_CHAMBER);
            foreach (Player player in players)
            {
                if (player.UserId == _mainPeanut.UserId)
                {
                    player.SetRole(RoleType.Scp173);
                    yield return Timing.WaitForSeconds(0.3f);
                    player.Position = peanutStart.Position + new Vector3(0, 2);
                    yield return Timing.WaitForSeconds(1f);
                    player.MaxHealth = 4000;
                    player.Health  = 1000;
                }
                else
                {
                    player.SetRole(RoleType.ClassD);
                    yield return Timing.WaitForSeconds(0.3f);
                    player.Position = dStart.Position + new Vector3(0, 2);
                }
            }
            yield return Timing.WaitForSeconds(0.3f);
            Util.MapUtil.DisableElevators();
            //Map.StartDecontamination();
            Exiled.Events.Handlers.Player.Died += OnPeanutKillDelegate;
            Exiled.Events.Handlers.Player.Spawning += OnPlayerSpawn;
        }

        private void OnPlayerSpawn(SpawningEventArgs ev)
        {
            Timing.RunCoroutine(OnPlayerSpawnRoutine(ev));
        }

        private IEnumerator<float> OnPlayerSpawnRoutine(SpawningEventArgs ev)
        {
            yield return Timing.WaitForSeconds(0.3f);
            if (ev.RoleType == RoleType.Scp173) yield break;
            
            ev.Player.SetRole(RoleType.ClassD);
            yield return Timing.WaitForSeconds(0.3f);
            ev.Player.Position = Util.MapUtil.GetRoomByName(Constant.SEVEN_NINE_CHAMBER).Position + new Vector3(0, 2);
        }

        private void OnPeanutKillDelegate(DiedEventArgs ev)
        {
            Timing.RunCoroutine(OnPeanutKill(ev));
        }

        private IEnumerator<float> OnPeanutKill(DiedEventArgs ev)
        {
            yield return Timing.WaitForSeconds(1f);
            if (ev.Killer.UserId == _mainPeanut.UserId)
            {
                ev.Target.SetRole(RoleType.Scp173);
                yield return Timing.WaitForSeconds(1f);
                ev.Target.Scale = new Vector3(0.5f, 0.5f, 0.5f);
                yield return Timing.WaitForSeconds(1f);
                ev.Target.Position = ev.Killer.Position;
                ev.Target.MaxHealth = 800;
                ev.Target.Health = 400;
            }
            
            if (Player.List.All(player => player.Role != RoleType.ClassD))
            {
                Util.PlayerUtil.GlobalBroadcast(15, "All ClassD has been eliminated. SCP 173 wins!");
                Util.RoundUtils.ForceEndRound(RoleType.Scp173);
            }
            
            if (Player.List.All(player => player.Role != RoleType.Scp173))
            {
                Util.PlayerUtil.GlobalBroadcast(15, "All SCP 173 has been eliminated. Humans win!");
                Util.RoundUtils.ForceEndRound(RoleType.ClassD);
            }
        }

        public override void OnRoundEnd()
        {
            Exiled.Events.Handlers.Player.Died -= OnPeanutKillDelegate;
            Exiled.Events.Handlers.Server.RestartingRound -= RoundRestartEvent;
            Exiled.Events.Handlers.Server.RespawningTeam -= Util.RoundUtils.PreventRespawnEvent;
            Exiled.Events.Handlers.Player.Spawning -= OnPlayerSpawn;
        }

        public void RoundRestartEvent()
        {
            OnRoundEnd();
        }

        public override void Disable()
        {
            OnRoundEnd();
        }
    }
}