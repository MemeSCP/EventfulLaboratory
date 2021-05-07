using System;
using CommandSystem;
using EventfulLaboratory.slevents;
using EventfulLaboratory.structs;
using Exiled.Permissions.Extensions;

namespace EventfulLaboratory.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class SetEvent : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!(sender as CommandSender).CheckPermission("el.event"))
            {
                response = "No permission!";
                return false;
            }
            LabEvents evnt;
            if (arguments.Count == 0)
            {
                response = "Avaliable events: //TODO";
                return true;
            }
            if (!Enum.TryParse(arguments.At(0), out evnt))
            {
                response = "First argument is not a number! Please provide a valid eventId. (cmd: event <number>)";
                return false;
            }
            EventfulLab.NextEvent = AEvent.GetEvent(evnt);
            if (EventfulLab.NextEvent == null || EventfulLab.NextEvent.GetType() == typeof(BlankEvent))
            {
                response = "Event with id: " + arguments.At(0) + " was not found!";
                return false;
            }
            response = "Event " + EventfulLab.NextEvent.GetName() + " will be played on next round.";
            Common.Broadcast(10, "Next event round: " + EventfulLab.NextEvent.GetName(), true);
             return true;
        }

        public string Command { get; } = "event";
        public string[] Aliases { get; } = {"setevent", "sevent"};
        public string Description { get; } = "Sets the next round's event.";
    }
}