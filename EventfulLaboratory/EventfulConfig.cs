using Exiled.API.Interfaces;

namespace EventfulLaboratory
{
    public sealed class EventfulConfig : IConfig
    {
        public bool IsEnabled { get; set; }

        public bool DebugMode { get; set; } = false;

        public bool RandomEvents { get; set; } = false;
    }
}