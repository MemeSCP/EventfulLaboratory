using System;
using CommandSystem;
using EventfulLaboratory.slevents;
using EventfulLaboratory.structs;
using Exiled.Permissions.Extensions;
using Random = UnityEngine.Random;

namespace EventfulLaboratory.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    internal sealed class SetEvent : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!(sender as CommandSender).CheckPermission("el.event"))
            {
                response = "No permission!";
                return false;
            }
            LabEvents evnt;
            if (arguments.Count == 0 || arguments.At(0) == "help" || arguments.At(0) == "list")
            {
                response = "Avaliable events:";
                foreach (var ev in Enum.GetNames(typeof(LabEvents)))
                {
                    response += "\n" + ev + ": " + Enum.Parse(typeof(LabEvents), ev);
                }
                return true;
            }

            if (arguments.At(0) == "random")
            {
                EventfulLab.NextEvent = AEvent.GetRandomEvent();
            }
            else if (Enum.TryParse(arguments.At(0), out evnt))
            {
                EventfulLab.NextEvent = AEvent.GetEvent(evnt);
            }
            else
            {
                response = "First argument is not a number! Please provide a valid eventId. (cmd: event <number>)";
                return false;
            }

            if (EventfulLab.NextEvent == null || EventfulLab.NextEvent.GetType() == typeof(BlankEvent))
            {
                response = "Event with id: " + arguments.At(0) + " was not found!";
                return false;
            }
            response = "Event " + EventfulLab.NextEvent.GetName() + " will be played on next round.";
            Util.PlayerUtil.GlobalBroadcast(10, "Next event round: " + EventfulLab.NextEvent.GetName(), true);
             return true;
        }

        public string Command { get; } = "event";
        public string[] Aliases { get; } = {"setevent", "sevent"};
        public string Description { get; } = "Sets the next round's event.";
    }
}