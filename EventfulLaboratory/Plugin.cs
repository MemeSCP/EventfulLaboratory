using System;
using EventfulLaboratory.structs;
using EventfulLaboratory.slevents;
using EXILED;
using EXILED.Extensions;
using EXILED.Patches;
using UnityEngine;

namespace EventfulLaboratory
{
    public class Plugin : EXILED.Plugin
    {
        private static AEvent _eventCandidate;
        private static AEvent _nextEvent;
        public static readonly bool DEBUG = true;
        private static bool _doRandom = false;

        public override void OnEnable()
        {
            if (_eventCandidate == null)
            {
                _eventCandidate = new DebugEvent();
            }
            
            Events.RoundRestartEvent += OnNewRound;
            Events.RoundStartEvent += OnRoundStart;
            Events.RoundEndEvent += OnRoundEnd;
            Events.RemoteAdminCommandEvent += OnCommand;
            _eventCandidate.Enable();
        }

        public override void OnDisable()
        {
            Events.RoundRestartEvent -= OnNewRound;
            Events.RoundStartEvent -= OnRoundStart;
            Events.RoundEndEvent -= OnRoundEnd;
            Events.RemoteAdminCommandEvent -= OnCommand;
            _eventCandidate?.Disable();
        }

        public override void OnReload()
        {
            OnDisable();
            OnEnable();
        }

        private void OnRoundStart() => _eventCandidate?.OnRoundStart();

        private void OnRoundEnd() => _eventCandidate?.OnRoundEnd();

        private void OnNewRound()
        {
            if (_doRandom && _nextEvent == null)
            {
                _nextEvent = AEvent.GetRandomEvent();
            }
            if (_nextEvent != null)
            {
                _eventCandidate.Disable();
                _eventCandidate = _nextEvent;
                _nextEvent = null;
                _eventCandidate.Enable();
            }
            _eventCandidate?.OnNewRound();
        }

        public override string getName => Constant.PLUGIN_NAME;

        private void OnCommand(ref RACommandEvent ev)
        {
            string[] args = ev.Command.Split(' ');
            ReferenceHub user = Player.GetPlayer(ev.Sender.SenderId);
            switch (args[0])
            {
                case "event":
                    if (user.CheckPermission("el.event"))
                    {
                        LabEvents evnt;
                        if (!Enum.TryParse(args[1], out evnt))
                        {
                            ev.Sender.RAMessage("First argument is not a number! Please provide a valid eventId. (cmd: event <number>)", false);
                            return;
                        }
                        _nextEvent = AEvent.GetEvent(evnt);
                        if (_nextEvent == null || _nextEvent.GetType() == typeof(BlankEvent))
                        {
                            ev.Sender.RAMessage("Event with id: " + args[1] + " was not found!", false);
                        }
                        else
                        {
                            ev.Sender.RAMessage("Event " + _nextEvent.GetName() + " will be played on next round.");
                            Common.Broadcast(10, "Next event round: " + _nextEvent.GetName(), true);
                        }
                    }
                    else
                    {
                        ev.Sender.RAMessage("You don't have permission to use this command!", false, "");
                    }
                    break;
                case "sqdebug":
                    switch (args[1])
                    {
                        case "forward":
                            user.SetPosition(user.GetPosition() + ((Common.GetEvacuationZone()?.Transform.forward ?? new Vector3()) * int.Parse(args[2])));
                            break;
                        case "evac":
                            user.SetPosition(Common.GetEvacuationZone()?.Position ?? user.GetPosition() + new Vector3(0, 2 ,0));
                            break;
                    }
                    break;
            }
        }
    }
}