using Exiled.API.Interfaces;

namespace EventfulLaboratory
{
    public sealed class EventfulConfig : IConfig
    {
        public bool IsEnabled { get; set; }

        public bool DevelopmentMode { get; set; } = false;

        public bool RandomEvents { get; set; } = false;

        public int PermanentEvents { get; set; } = 0;

        public bool EnableTagModifications { get; set; } = true;
    }
}