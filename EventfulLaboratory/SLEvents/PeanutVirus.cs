using System.Collections.Generic;
using System.Linq;
using EventfulLaboratory.structs;
using EXILED;
using EXILED.ApiObjects;
using EXILED.Extensions;
using MEC;
using UnityEngine;
using Random = System.Random;

namespace EventfulLaboratory.slevents
{
    public class PeanutVirus : AEvent
    {
        private ReferenceHub _mainPeanut;
        
        public override void OnNewRound()
        {
            Common.LockRound();

            Events.RoundRestartEvent += RoundRestartEvent;
            Events.TeamRespawnEvent += Common.PreventRespawnEvent;
        }

        public override void OnRoundStart()
        {
            Common.Broadcast(15, "Welcome to Peanut Virus. Avoid SCP 173. On death consequences happen.");
            Timing.RunCoroutine(RoundStartEnumerator());    
        }

        private IEnumerator<float> RoundStartEnumerator()
        {
            yield return Timing.WaitForSeconds(1f);
            List<ReferenceHub> players = Player.GetHubs().ToList();
            _mainPeanut = players[new Random().Next(players.Count)];
            Room peanutStart = Common.GetRoomByName(Constant.FOUR_NINE_CHAMBER);
            Room dStart = Common.GetRoomByName(Constant.SEVEN_NINE_CHAMBER);
            foreach (ReferenceHub hub in players)
            {
                if (hub == _mainPeanut)
                {
                    hub.SetRole(RoleType.Scp173);
                    yield return Timing.WaitForSeconds(0.3f);
                    hub.SetPosition(peanutStart.Position + new Vector3(0, 2));
                    yield return Timing.WaitForSeconds(1f);
                    hub.SetMaxHealth(4000);
                    hub.SetHealth(1000);
                }
                else
                {
                    hub.SetRole(RoleType.ClassD);
                    yield return Timing.WaitForSeconds(0.3f);
                    hub.SetPosition(dStart.Position + new Vector3(0, 2));
                }
            }
            yield return Timing.WaitForSeconds(0.3f);
            Map.StartDecontamination();
            Events.PlayerDeathEvent += OnPeanutKillDelegate;
            Events.PlayerSpawnEvent += OnPlayerSpawn;
        }

        private void OnPlayerSpawn(PlayerSpawnEvent ev)
        {
            Timing.RunCoroutine(OnPlayerSpawnRoutine(ev));
        }

        private IEnumerator<float> OnPlayerSpawnRoutine(PlayerSpawnEvent ev)
        {
            yield return Timing.WaitForSeconds(0.3f);
            if (ev.Role != RoleType.Scp173)
            {
                ev.Player.SetRole(RoleType.ClassD);
                yield return Timing.WaitForSeconds(0.3f);
                ev.Player.SetPosition(Common.GetRoomByName(Constant.SEVEN_NINE_CHAMBER).Position + new Vector3(0, 2));
            }
        }

        private void OnPeanutKillDelegate(ref PlayerDeathEvent ev)
        {
            Timing.RunCoroutine(OnPeanutKill(ev));
        }

        private IEnumerator<float> OnPeanutKill(PlayerDeathEvent ev)
        {
            if (ev.Killer == _mainPeanut)
            {
                ev.Player.SetRole(RoleType.Scp173);
                yield return Timing.WaitForSeconds(1f);
                ev.Player.SetScale(0.5f);
                ev.Player.SetPosition(ev.Killer.GetPosition());
                ev.Player.SetMaxHealth(800);
                ev.Player.SetHealth(400);
            }
            
            if (Player.GetHubs().All(player => player.GetRole() != RoleType.ClassD))
            {
                Common.Broadcast(15, "All ClassD has been eliminated. SCP 173 wins!");
                Common.ForceRoundEnd(RoundSummary.LeadingTeam.Anomalies);
            }
            
            if (Player.GetHubs().All(player => player.GetRole() != RoleType.Scp173))
            {
                Common.Broadcast(15, "All SCP 173 has been eliminated. Humans win!");
                Common.ForceRoundEnd(RoundSummary.LeadingTeam.ChaosInsurgency);
            }
        }

        public override void OnRoundEnd()
        {
            Events.PlayerDeathEvent -= OnPeanutKillDelegate;
            Events.RoundRestartEvent -= RoundRestartEvent;
            Events.TeamRespawnEvent -= Common.PreventRespawnEvent;
            Events.PlayerSpawnEvent -= OnPlayerSpawn;
        }

        public void RoundRestartEvent()
        {
            OnRoundEnd();
        }

        public override void Enable()
        {
            //NOOP
        }

        public override void Disable()
        {
            OnRoundEnd();
        }

        public override void Reload()
        {
            //NOOP
        }
    }
}