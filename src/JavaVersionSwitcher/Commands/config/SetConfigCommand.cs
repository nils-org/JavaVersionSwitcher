using System.ComponentModel;
using System.Threading.Tasks;
using JavaVersionSwitcher.Logging;
using JavaVersionSwitcher.Services;
using JetBrains.Annotations;
using Spectre.Console.Cli;

namespace JavaVersionSwitcher.Commands.config;

[UsedImplicitly]
public class SetConfigCommand : AsyncCommand<SetConfigCommand.Settings>
{
    private readonly ILogger _logger;
    private readonly IConfigurationService _service;

    public SetConfigCommand(
        ILogger logger,
        IConfigurationService service)
    {
        _logger = logger;
        _service = service;
    }
        
    [UsedImplicitly]
    public sealed class Settings : CommonConfigCommandSettings
    {
        [CommandArgument(2, "[Value]")]
        [Description("The value to set.")]
        [UsedImplicitly]
        public string Value { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        _logger.PrintVerbose = settings.Verbose;

        await _service.SetConfiguration(settings.Provider, settings.Name, settings.Value);

        return 0;
    }
}