using System;
using EventfulLaboratory.structs;
using EventfulLaboratory.slevents;
using Exiled.API.Features;
using Exiled.Events;
using Exiled.Events.EventArgs;
using UnityEngine;

namespace EventfulLaboratory
{
    public class EventfulLab : Plugin<EventfulConfig>
    {
        private static readonly Lazy<EventfulLab> LazyInstance = new Lazy<EventfulLab>(() => new EventfulLab());

        public static EventfulLab Instance => LazyInstance.Value;

        private static AEvent _eventCandidate;
        public static AEvent NextEvent { get; set; }

        public override void OnEnabled()
        {
            if (_eventCandidate == null)
            {
                _eventCandidate = AEvent.GetEvent((LabEvents)Config.PermanentEvents);
            }
            
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnNewRound;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnd;
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnNewRound;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnd;
            _eventCandidate?.Disable();
        }

        public override void OnReloaded()
        {
            OnDisabled();
            OnEnabled();
        }

        private void OnRoundStart() => _eventCandidate?.OnRoundStart();

        private void OnRoundEnd(RoundEndedEventArgs args) => _eventCandidate?.OnRoundEnd();

        private void OnNewRound()
        {
            if (Config.RandomEvents)
            {
                NextEvent = AEvent.GetRandomEvent();
            }
            else if (NextEvent != null)
            {
                _eventCandidate?.Disable();
                _eventCandidate = NextEvent;
                NextEvent = null;
                _eventCandidate.Enable();
            }
            _eventCandidate?.OnNewRound();
        }
    }
}