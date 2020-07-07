using System;
using System.Collections.Generic;
using System.Linq;
using EventfulLaboratory.structs;
using EXILED;
using EXILED.ApiObjects;
using EXILED.Extensions;
using MEC;
using UnityEngine;
using YamlDotNet.Core;
using Random = System.Random;

namespace EventfulLaboratory.slevents
{
    public class PeanutChamber : AEvent
    {
        private Room _shelter;
        
        public override void OnNewRound()
        {
            //noop
        }

        public override void OnRoundStart()
        {
            Events.TeamRespawnEvent += Common.PreventRespawnEvent;
            Common.LockRound();
            Common.LockAllDoors();
            _shelter = Common.GetEvacuationZone();
            Events.PlayerJoinEvent += PreRoundJoin;
            if (_shelter != null)
            {
                foreach (ReferenceHub hub in Player.GetHubs())
                {
                    Timing.RunCoroutine(MovePlayerToShelter(hub));
                }
            }

            Timing.RunCoroutine(FewSecToRound());
        }

        private void SendWelcomeMessage(ReferenceHub hub)
        {
            hub.Broadcast(2, Constant.PEANUT_CHAMBER_WELCOME, false);
        }

        private void PreRoundJoin(PlayerJoinEvent ev)
        {
            Timing.RunCoroutine(MovePlayerToShelter(ev.Player));
        }

        private void OnKill(ref PlayerDeathEvent ev)
        {
            ReferenceHub dboy = null;
            foreach (ReferenceHub hub in Player.GetHubs())
            {
                if (hub == ev.Player || hub == ev.Killer) continue;
                if (hub.GetRole() == RoleType.ClassD)
                {
                    if (dboy == null)
                    {
                        //Found 1 dboy
                        dboy = hub;
                    }
                    else
                    {
                        //Found 2, skipping rest of OnKill event
                        return;
                    }
                }
            }
            //Only 0/1 dboy exists
            if (dboy != null)
            {
                dboy.Broadcast(3, Constant.PEANUT_CHAMBER_DBOY_WIN, false);
                foreach (ReferenceHub hub in Player.GetHubs())
                {
                    if (hub.GetRole() == RoleType.Spectator || hub == ev.Player)
                    {
                        Timing.RunCoroutine(SetPeanutCoroutine(hub));
                        hub.Broadcast(3, Constant.PEANUT_CHAMBER_DBOY_ELLAMINATE, false);
                    }
                }
            }
            else
            {
                Common.Broadcast(3, Constant.PEANUT_CHAMBER_END);
                Map.RoundLock = false;
            }
        }

        public override void OnRoundEnd()
        {
            Events.PlayerDeathEvent -= OnKill;
            Events.TeamRespawnEvent -= Common.PreventRespawnEvent;
        }

        public override void Enable()
        {
            
        }

        public override void Disable()
        {
            OnRoundEnd();
        }

        public override void Reload()
        {
            
        }
        
        private IEnumerator<float> FewSecToRound()
        {
            yield return Timing.WaitForSeconds(3f);
            for (int i = 10; i > 0; i--)
            {
                Common.Broadcast(1, "Round starts in " + i + " seconds.");
                yield return Timing.WaitForSeconds(1);
            }
            yield return Timing.WaitForSeconds(3f);
            TheRound();
        }
        
        private IEnumerator<float> MovePlayerToShelter(ReferenceHub hub)
        {
            yield return Timing.WaitForSeconds(1.5f);
            hub.SetRole(RoleType.ClassD);
            yield return Timing.WaitForSeconds(0.5f);
            hub.SetPosition(_shelter.Position + new Vector3(0, 2, 0));
            SendWelcomeMessage(hub);
        }
        
        private void TheRound()
        {
            EXILED.Events.PlayerJoinEvent -= PreRoundJoin;
            List<ReferenceHub> players = Player.GetHubs().ToList();
            ReferenceHub thePeanut = players[new Random().Next(players.Count)];
            players.Remove(thePeanut);
            Timing.RunCoroutine(SetPeanutCoroutine(thePeanut));
            players.ForEach(player => player.Broadcast(4, Constant.PEANUT_CHAMBER_DCLASS_WARN, false));
            EXILED.Events.PlayerDeathEvent += OnKill;
        }

        private IEnumerator<float> SetPeanutCoroutine(ReferenceHub hub)
        {
            yield return Timing.WaitForSeconds(0.3f);
            hub.SetRole(RoleType.Scp173);
            yield return Timing.WaitForSeconds(0.3f);
            hub.SetPosition(_shelter.Position + new Vector3(0, 2, 0));
            hub.SetMaxHealth(5000);
            hub.SetHealth(1000);
            hub.Broadcast(15, Constant.PEANUT_CHAMBER_173_WARN, false);
        }
    }
}