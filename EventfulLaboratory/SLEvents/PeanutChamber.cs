using System;
using System.Collections.Generic;
using System.Linq;
using EventfulLaboratory.structs;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using UnityEngine;
using YamlDotNet.Core;
using Random = System.Random;

namespace EventfulLaboratory.slevents
{
    public class PeanutChamber : AEvent
    {
        private Room _shelter;
        
        public override void OnRoundStart()
        {
            Exiled.Events.Handlers.Server.RespawningTeam += Common.PreventRespawnEvent;
            Common.LockRound();
            Common.LockAllDoors();
            //TODO: Replace Shelter with other place
            _shelter = Common.GetEvacuationZone();
            Exiled.Events.Handlers.Player.Joined += PreRoundJoin;
            if (_shelter != null)
            {
                foreach (Player player in Player.List)
                {
                    //Move to 173 chamber
                    Timing.RunCoroutine(MovePlayerToShelter(player));
                }
            }

            Timing.RunCoroutine(FewSecToRound());
        }

        private void SendWelcomeMessage(Player player)
        {
            player.Broadcast(4, Constant.PEANUT_CHAMBER_WELCOME);
        }

        private void PreRoundJoin(JoinedEventArgs ev)
        {
            Timing.RunCoroutine(MovePlayerToShelter(ev.Player));
        }

        private void OnKill(DiedEventArgs ev)
        {
            Player dboy = null;
            foreach (Player player in Player.List)
            {
                if (player == ev.Target || player == ev.Killer) continue;
                if (player.Role == RoleType.ClassD)
                {
                    if (dboy == null)
                    {
                        //Found 1 dboy
                        dboy = player;
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
                dboy.Broadcast(3, Constant.PEANUT_CHAMBER_DBOY_WIN);
                foreach (Player player in Player.List)
                {
                    if (player.Role == RoleType.Spectator || player == ev.Target)
                    {
                        Timing.RunCoroutine(SetPeanutCoroutine(player));
                        player.Broadcast(3, Constant.PEANUT_CHAMBER_DBOY_ELLAMINATE);
                    }
                }
            }
            else
            {
                Common.Broadcast(3, Constant.PEANUT_CHAMBER_END);
                Common.LockRound(false);
            }
        }

        public override void OnRoundEnd()
        {
            Exiled.Events.Handlers.Player.Died -= OnKill;
            Exiled.Events.Handlers.Server.RespawningTeam -= Common.PreventRespawnEvent;
        }

        public override void Disable()
        {
            OnRoundEnd();
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
        
        private IEnumerator<float> MovePlayerToShelter(Player player)
        {
            yield return Timing.WaitForSeconds(1.5f);
            player.SetRole(RoleType.ClassD);
            yield return Timing.WaitForSeconds(0.5f);
            player.Position = _shelter.Position + new Vector3(0, 2, 0);
            SendWelcomeMessage(player);
        }
        
        private void TheRound()
        {
            Exiled.Events.Handlers.Player.Joined -= PreRoundJoin;
            List<Player> players = Player.List.ToList();
            Player thePeanut = players[new Random().Next(players.Count)];
            players.Remove(thePeanut);
            Timing.RunCoroutine(SetPeanutCoroutine(thePeanut));
            players.ForEach(player => player.Broadcast(4, Constant.PEANUT_CHAMBER_DCLASS_WARN));
            Exiled.Events.Handlers.Player.Died += OnKill;
        }

        private IEnumerator<float> SetPeanutCoroutine(Player player)
        {
            yield return Timing.WaitForSeconds(0.3f);
            player.SetRole(RoleType.Scp173);
            yield return Timing.WaitForSeconds(0.3f);
            player.Position = _shelter.Position + new Vector3(0, 2, 0);
            player.MaxHealth = 2500;
            player.Health = 1000;
            player.Broadcast(15, Constant.PEANUT_CHAMBER_173_WARN);
        }
    }
}