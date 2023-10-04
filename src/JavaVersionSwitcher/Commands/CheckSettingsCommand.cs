using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JavaVersionSwitcher.Adapters;
using JavaVersionSwitcher.Logging;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace JavaVersionSwitcher.Commands;

[UsedImplicitly]
internal sealed class CheckSettingsCommand : AsyncCommand<CheckSettingsCommand.Settings>
{
    private readonly IJavaHomeAdapter _javaHomeAdapter;
    private readonly IJavaInstallationsAdapter _javaInstallationsAdapter;
    private readonly IPathAdapter _pathAdapter;
    private readonly IRegistryAdapter _registryAdapter;
    private readonly ILogger _logger;
    private readonly IAnsiConsole _console;

    public CheckSettingsCommand(
        IJavaHomeAdapter javaHomeAdapter,
        IJavaInstallationsAdapter javaInstallationsAdapter,
        IPathAdapter pathAdapter,
        IRegistryAdapter registryAdapter,
        ILogger logger,
        IAnsiConsole console
    )
    {
        _javaHomeAdapter = javaHomeAdapter;
        _javaInstallationsAdapter = javaInstallationsAdapter;
        _pathAdapter = pathAdapter;
        _registryAdapter = registryAdapter;
        _logger = logger;
        _console = console;
    }

    [UsedImplicitly]
    public sealed class Settings : CommonCommandSettings
    {
        /*
         TODO: Implement
        [CommandOption("--fix")]
        [Description("if set, any found problem is attempted to be fixed.")]
        [DefaultValue(false)]
        public bool Fix { get; init; }
        */
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        _logger.PrintVerbose = settings.Verbose;
        var javaHome = await _javaHomeAdapter.GetValue(EnvironmentScope.Process);
        if (string.IsNullOrEmpty(javaHome))
        {
            _console.MarkupLine("[red]JAVA_HOME is not set[/] JAVA_HOME needs to be set, for PATH check to work.");
            return 1;
        }

        _console.MarkupLine("[green]JAVA_HOME[/]: " + javaHome);

        var javaInstallations = await _javaInstallationsAdapter
            .GetJavaInstallations()
            .ConfigureAwait(false);
        var paths = (await _pathAdapter.GetValue(EnvironmentScope.Process)).ToList();

        var errors = false;
        var javaHomeBin = Path.Combine(javaHome, "bin");
        var javaHomeInPath = paths.FirstOrDefault(x =>
            x.StartsWith(javaHomeBin, StringComparison.OrdinalIgnoreCase) &&
            (x.Length == javaHomeBin.Length || x.Length == javaHomeBin.Length + 1));
        if (javaHomeInPath != null)
        {
            _console.MarkupLine("[green]JAVA_HOME\\bin is in PATH[/]: " + javaHomeInPath);
        }
        else
        {
            errors = true;
            _console.MarkupLine(@"[red]JAVA_HOME\bin is not in PATH[/] JAVA_HOME\bin needs to be in PATH.");
        }

        foreach (var java in javaInstallations)
        {
            var javaInstallationsInPath = paths
                .Where(x => x.StartsWith(java.Location, StringComparison.OrdinalIgnoreCase))
                .Where(x => !x.Equals(javaHomeInPath));

            foreach (var path in javaInstallationsInPath)
            {
                errors = true;
                _console.MarkupLine($"[red]Additional java in PATH[/]: {path}");
            }
        }

        await foreach (var path in _registryAdapter.GetInstallationPaths())
        {
            var binPath = Path.Combine(path, "bin", "java.exe");
            if (File.Exists(binPath))
            {
                continue;
            }
            
            var warn =
                $"Path for java installation '{path}' set in  Windows Registry. But does not contain a java executable!";
            _console.MarkupLine($"[red]{warn}[/]");
        }

        if (errors)
        {
            _console.MarkupLine("[yellow]There were errors.[/]");
            _console.MarkupLine("[yellow]Fixing errors is not implemented. You need to fix them manually![/]");
            return 1;
        }

        return 0;
    }
}