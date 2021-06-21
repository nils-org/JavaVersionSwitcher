
using System.Collections.Generic;
using System.Threading.Tasks;
using JavaVersionSwitcher.Services;

namespace JavaVersionSwitcher.Tests.TestImplementations
{
    public class TestConfigurationService : IConfigurationService
    {
        public Dictionary<string, Dictionary<string, string>> Configuration =>
            new Dictionary<string, Dictionary<string, string>>();
        
        public Task<string> GetConfiguration(string provider, string settingName)
        {
            
            if (Configuration.TryGetValue(provider, out var settings))
            {
                if (settings.TryGetValue(settingName, out var value))
                {
                    return Task.FromResult(value);
                }
            }

            return Task.FromResult(string.Empty);
        }

        public Task SetConfiguration(string provider, string settingName, string value)
        {
            if (!Configuration.TryGetValue(provider, out var settings))
            {
                settings = new Dictionary<string, string>();
                Configuration.Add(provider, settings);
            }

            settings[settingName] = value;
            return Task.FromResult<object>(null);
        }
    }
}