using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JavaVersionSwitcher.Logging;

namespace JavaVersionSwitcher.Services
{
    /// <inheritdoc cref="IConfigurationProvider"/>
    public class JavaInstallationsAdapterConfigurationProvider : IConfigurationProvider
    {
        private readonly ILogger _logger;
        private const string Timeout = "timeout";
        private const string StartPaths = "startPaths";

        public JavaInstallationsAdapterConfigurationProvider(
            ILogger logger)
        {
            _logger = logger;
        }
        
        /// <inheritdoc cref="IConfigurationProvider.ProviderName"/>
        public string ProviderName => "scan";

        /// <inheritdoc cref="IConfigurationProvider.Settings"/>
        public IEnumerable<string> Settings => new []{ StartPaths, Timeout };
        
        public async Task<int> GetCacheTimeout(IConfigurationService service)
        {
            const int @default = 7;
            var providerName = ProviderName;
            var displayName = $"{providerName}:{Timeout}";
            var config = await service.GetConfiguration(
                providerName,
                Timeout);
            if (string.IsNullOrEmpty(config))
            {
                return @default;
            }

            if (int.TryParse(config, out var parsed))
            {
                _logger.LogVerbose($"Using configured value for {displayName} of {parsed} days.");
                return parsed;
            }
            
            _logger.LogWarning(
                $"Configured value for {displayName} of {config} could not be parsed.");
            return @default;
        }
    
        public async Task<IEnumerable<string>> GetStartPaths(IConfigurationService service)
        {
            var providerName = ProviderName;
            var displayName = $"{providerName}:{StartPaths}";
            var @default = new[]
            {
                "%ProgramW6432%",
                "%ProgramFiles(x86)%"
            };
            
            var config = await service.GetConfiguration(providerName, StartPaths);
            if (string.IsNullOrEmpty(config))
            {
                return @default;
            }

            var parts = config.Split(";", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                _logger.LogWarning(
                    $"Configured value for {displayName} of '{config}' results in an empty list.");
                return @default;
            }

            _logger.LogVerbose($"Using configured values for {displayName} ");
            parts.ToList().ForEach(x => _logger.LogVerbose($" - {x}"));
            return parts;
        }

    }
}