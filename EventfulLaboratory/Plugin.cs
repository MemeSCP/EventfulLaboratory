using EventfulLaboratory.structs;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Server = Exiled.Events.Handlers.Server;

namespace EventfulLaboratory
{
    public class EventfulLab : Plugin<EventfulConfig>
    {
        public override string Name => "Eventful Laboratory";
        public override string Author => "MemeSCP";

        private static EventfulLab _instance;
        public static EventfulLab Instance => _instance;

        private static AEvent _eventCandidate;
        public static AEvent NextEvent { get; set; }

        public override void OnEnabled()
        {
            _instance = this;
            
            Log.Info("Starting Eventful Laboratory.");
            if (_eventCandidate == null)
            {
                _eventCandidate = AEvent.GetEvent((LabEvents)Config.PermanentEvents);
            }
            
            Server.WaitingForPlayers += OnNewRound;
            Server.RoundStarted += OnRoundStart;
            Server.RoundEnded += OnRoundEnd;
            
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Server.WaitingForPlayers -= OnNewRound;
            Server.RoundStarted -= OnRoundStart;
            Server.RoundEnded -= OnRoundEnd;
            _eventCandidate?.Disable();

            _instance = null;
            
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