using System.Collections.Generic;
using System.Linq;
using EventfulLaboratory.structs;
using Exiled.API.Features;

namespace EventfulLaboratory.SLEvents
{
    internal sealed class TagEvent : AEvent
    {

        private List<Player> _currentIts = new List<Player>();
        private List<Player> _cooldowns = new List<Player>();
        
        public override void OnRoundStart()
        {
            var count = Player.List.Count();
            
        }
    }
}