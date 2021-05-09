using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EventfulLaboratory.Handler
{
    public class PreEventConfigureHandler
    {
        private readonly Dictionary<string, string> _config;
        
        public PreEventConfigureHandler(IEnumerable<string> configurableKeys)
        {
            _config = configurableKeys.ToDictionary(s => s);
        }

        public string List() => _config.Aggregate("", (current, keyValuePair) => current + $"{{keyValuePair.Key}}={keyValuePair.Value}");
        
    }
}