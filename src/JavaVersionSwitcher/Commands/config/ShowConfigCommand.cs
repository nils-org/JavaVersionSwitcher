using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using JavaVersionSwitcher.Logging;
using JavaVersionSwitcher.Services;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace JavaVersionSwitcher.Commands.config
{
    [UsedImplicitly]
    public class ShowConfigCommand : AsyncCommand<ShowConfigCommand.Settings>
    {
        private readonly ILogger _logger;
        private readonly IConfigurationService _service;
        private readonly IEnumerable<IConfigurationProvider> _providers;
        private readonly IAnsiConsole _console;

        public ShowConfigCommand(
            ILogger logger,
            IConfigurationService service,
            IEnumerable<IConfigurationProvider> providers,
            IAnsiConsole console)
        {
            _logger = logger;
            _service = service;
            _providers = providers;
            _console = console;
        }
        
        [UsedImplicitly]
        public sealed class Settings : CommonCommandSettings
        {
            [CommandOption("--providers")]
            [Description("Show provider names only.")]
            [DefaultValue(false)]
            public bool Providers { get; [UsedImplicitly] set; }

            [CommandOption("--provider")]
            [Description("Show settings for one provider only.")]
            public string Provider { get; [UsedImplicitly] set; }

            public override ValidationResult Validate()
            {
                if (Providers && !string.IsNullOrEmpty(Provider))
                {
                    return ValidationResult.Error("--providers and --provider are mutually exclusive.");
                }
                
                return ValidationResult.Success();
            }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            _logger.PrintVerbose = settings.Verbose;
            if (settings.Providers)
            {
                return await ListProviders();
            }

            if (!string.IsNullOrEmpty(settings.Provider))
            {
                return await ListSettingsForProviders(settings.Provider);
            }

            return await ListAllSettings();
        }

        private async Task<int> ListSettingsForProviders(string settingsProvider)
        {
            var provider = _providers.FirstOrDefault(p =>
                p.ProviderName.Equals(settingsProvider, StringComparison.OrdinalIgnoreCase));
            if (provider == null)
            {
                _console.MarkupLine($"[red]No provider named {settingsProvider}[/]");
                return await Task.FromResult(1);
            }
            
            _logger.LogVerbose($"Listing setting for {provider.ProviderName}:");
            foreach (var setting in provider.Settings.OrderBy(x => x))
            {
                _console.WriteLine(setting);
            }
            
            return await Task.FromResult(0);
        }

        private async Task<int> ListAllSettings()
        {
            var table = new Table();
            table.AddColumn("Provider");
            table.AddColumn("Setting");
            table.AddColumn("Configured value");

            foreach (var provider in _providers.OrderBy(p => p.ProviderName))
            {
                foreach (var setting in provider.Settings.OrderBy(x => x))
                {
                    var val = await _service.GetConfiguration(provider.ProviderName, setting);
                    table.AddRow(provider.ProviderName, setting, val);
                }
            }
            
            _console.Write(table);
            return 0;
        }

        private async Task<int> ListProviders()
        {
            foreach (var name in _providers.Select(p => p.ProviderName).OrderBy(x => x))
            {
                _console.WriteLine(name);
            }
            
            return await Task.FromResult(0);
        }
    }
}