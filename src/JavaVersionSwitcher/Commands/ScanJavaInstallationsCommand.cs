using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using JavaVersionSwitcher.Adapters;
using JavaVersionSwitcher.Logging;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace JavaVersionSwitcher.Commands;

[UsedImplicitly]
internal sealed class ScanJavaInstallationsCommand : AsyncCommand<ScanJavaInstallationsCommand.Settings>
{
    private readonly IJavaInstallationsAdapter _javaInstallationsAdapter;
    private readonly ILogger _logger;
    private readonly IAnsiConsole _console;

    public ScanJavaInstallationsCommand(
        IJavaInstallationsAdapter javaInstallationsAdapter,
        ILogger logger,
        IAnsiConsole console)
    {
        _javaInstallationsAdapter = javaInstallationsAdapter;
        _logger = logger;
        _console = console;
    }
        
    [UsedImplicitly]
    public sealed class Settings : CommonCommandSettings
    {
        [CommandOption("--force")]
        [DefaultValue(false)]
        [Description("Force re-scan and do not use cached data.")]
        public bool Force { get; [UsedImplicitly] set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        _logger.PrintVerbose = settings.Verbose;
        var installations = await _javaInstallationsAdapter
            .GetJavaInstallations(settings.Force)
            .ConfigureAwait(false);
        var table = new Table();
        table.AddColumn("Java path");
        table.AddColumn("version");
        foreach (var javaInstallation in installations.OrderBy(x => x.Location))
        {
            //var txt = javaInstallation.Version;
            //if(ver)
            table.AddRow(javaInstallation.Location, javaInstallation.Version ?? "unknown");
        }

        _console.Write(table);
        return 0;
    }
}