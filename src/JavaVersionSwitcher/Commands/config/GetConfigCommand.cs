using System.Threading.Tasks;
using JavaVersionSwitcher.Logging;
using JavaVersionSwitcher.Services;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace JavaVersionSwitcher.Commands.config
{
    [UsedImplicitly]
    public class GetConfigCommand : AsyncCommand<GetConfigCommand.Settings>
    {
        private readonly ILogger _logger;
        private readonly IConfigurationService _service;
        private readonly IAnsiConsole _console;

        public GetConfigCommand(
            ILogger logger,
            IConfigurationService service,
            IAnsiConsole console)
        {
            _logger = logger;
            _service = service;
            _console = console;
        }
        
        [UsedImplicitly]
        public sealed class Settings : CommonConfigCommandSettings
        {
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            _logger.PrintVerbose = settings.Verbose;
            
            var val = await _service.GetConfiguration(settings.Provider, settings.Name);
            _console.MarkupLine(val);

            return 0;
        }
    }
}