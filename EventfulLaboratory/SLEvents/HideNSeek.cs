using System;
using System.Collections.Generic;
using EventfulLaboratory.Extension;
using EventfulLaboratory.structs;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using UnityEngine;

namespace EventfulLaboratory.slevents
{
    public class HideNSeek : AEvent
    {
        private DateTime _roundStartTime;
        
        public override void OnRoundStart()
        {

            Exiled.Events.Handlers.Player.Joined += OnPlayerJoinProxy;
            Exiled.Events.Handlers.Server.RoundStarted += ServerOnRoundStarted;
        }

        private void ServerOnRoundStarted()
        {
            _roundStartTime = new DateTime();
        }

        #region Proxies :(

        private void OnPlayerJoinProxy(JoinedEventArgs ev) => Timing.RunCoroutine(OnPlayerJoined(ev));
        
        #endregion
        
        #region Exiled Events

        private IEnumerator<float> OnPlayerJoined(JoinedEventArgs ev)
        {
            yield return Timing.WaitForSeconds(0.1f);
            //Allow spawning 5 sec after roundstart
            if ((new DateTime() - _roundStartTime).TotalSeconds < 5)
            {
                ev.Player.SetRole(RoleType.Spectator);
                ev.Player.Broadcast(5, "Sorry, you have joined too late and was moved to Spectator.");
            }
            else
            {
                ev.Player.SetRole(RoleType.Spectator);
                ev.Player.Broadcast(5, "Sorry, you have joined too late and was moved to Spectator.");
            }
        }
        
        #endregion
        
        #region Helpers

        private IEnumerator<float> SpawnHider(Player ply)
        {
            yield return Timing.WaitForSeconds(0.1f);
            ply.Scale = new Vector3(0.3f, 0.3f, 0.3f);
            yield return Timing.WaitForSeconds(0.1f);
            ply.SetRole(RoleType.ClassD);
            yield return Timing.WaitForSeconds(0.3f);
            ply.MaxHealth = 1;
            ply.Health = 1;
            ply.ShowHint("HIDE! People will search for you!");
        }

        private IEnumerator<float> SpawnSeeker(Player ply)
        {
            yield return Timing.WaitForSeconds(0.1f);
            ply.SetRole(RoleType.FacilityGuard);
            
        }
        
        #endregion
    }
}