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

        public override string Name => "Eventful Laboratory";
        public override string Author => "Sqbika";

        public static EventfulLab Instance => LazyInstance.Value;

        private static AEvent _eventCandidate;
        public static AEvent NextEvent { get; set; }

        public override void OnEnabled()
        {
            Log.Info("Starting Eventful Laboratory.");
            if (_eventCandidate == null)
            {
                _eventCandidate = AEvent.GetEvent((LabEvents)Config.PermanentEvents);
            }
            
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnNewRound;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnd;
            
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnNewRound;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnd;
            _eventCandidate?.Disable();
            
            base.OnDisabled();
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
            if (Config.RandomEvents && NextEvent == null)
            {
                NextEvent = AEvent.GetRandomEvent();
            }
            if (NextEvent != null)
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