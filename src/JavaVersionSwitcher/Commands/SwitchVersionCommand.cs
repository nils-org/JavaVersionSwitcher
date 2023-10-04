using System;
using System.Collections.Generic;
using System.ComponentModel;
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
internal sealed class SwitchVersionCommand : AsyncCommand<SwitchVersionCommand.Settings>
{
    private readonly IJavaHomeAdapter _javaHomeAdapter;
    private readonly IJavaInstallationsAdapter _javaInstallationsAdapter;
    private readonly IPathAdapter _pathAdapter;
    private readonly ILogger _logger;
    private readonly IShellAdapter _shellAdapter;
    private readonly IAnsiConsole _console;

    public SwitchVersionCommand(
        IJavaHomeAdapter javaHomeAdapter,
        IJavaInstallationsAdapter javaInstallationsAdapter,
        IPathAdapter pathAdapter,
        ILogger logger,
        IShellAdapter shellAdapter,
        IAnsiConsole console
    )
    {
        _javaHomeAdapter = javaHomeAdapter;
        _javaInstallationsAdapter = javaInstallationsAdapter;
        _pathAdapter = pathAdapter;
        _logger = logger;
        _shellAdapter = shellAdapter;
        _console = console;
    }
        
    [UsedImplicitly]
    public sealed class Settings : CommonCommandSettings
    {
        [CommandOption("--machine")]
        [DefaultValue(false)]
        [Description("set variables in machine scope instead of user. needs admin privileges.")]
        public bool MachineScope { get; [UsedImplicitly] set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        _logger.PrintVerbose = settings.Verbose;
        var installations = await _javaInstallationsAdapter
            .GetJavaInstallations()
            .ConfigureAwait(false);

        var newJavaHome = _console.Prompt(
            new SelectionPrompt<string>()
                .Title("Which java should be set?")
                .PageSize(25)
                .MoreChoicesText("[grey](Move up and down to reveal more installations)[/]")
                .AddChoices(installations.Select(x => x.Location).ToArray()));
            
        string javaBin = null;
        await _console.Status()
            .StartAsync("Applying...", async ctx =>
            {
                ctx.Spinner(Spinner.Known.Star);
                ctx.SpinnerStyle(Style.Parse("green"));

                var scope = settings.MachineScope
                    ? EnvironmentScope.Machine
                    : EnvironmentScope.User;

                var oldJavaHome = await _javaHomeAdapter.GetValue(EnvironmentScope.Process);
                var paths = (await _pathAdapter.GetValue(scope)).ToList();
                if (!string.IsNullOrEmpty(oldJavaHome))
                {
                    paths = paths.Where(x => !x.StartsWith(oldJavaHome,StringComparison.OrdinalIgnoreCase)).ToList();
                }

                javaBin = Path.Combine(newJavaHome, "bin");
                paths.Add(javaBin);

                await _javaHomeAdapter.SetValue(newJavaHome, scope);
                await _pathAdapter.SetValue(paths, scope);
            }).ConfigureAwait(false);

        var shellType = _shellAdapter.GetShellType();
        var refreshCommands = new List<string>();
        switch (shellType)
        {
            case ShellType.PowerShell:
                refreshCommands.Add($"$env:JAVA_HOME=\"{newJavaHome}\"");
                refreshCommands.Add($"$env:PATH=\"{javaBin}{Path.PathSeparator}$($env:PATH)\"");
                break;
            case ShellType.CommandPrompt:
                refreshCommands.Add($"set \"JAVA_HOME={newJavaHome}\"");
                refreshCommands.Add($"set \"PATH={javaBin}{Path.PathSeparator}%PATH%\"");
                break;
        }

        _console.MarkupLine(refreshCommands.Count > 0
            ? "[yellow]The environment has been modified. Apply modifications:[/]"
            : "[yellow]The environment has been modified. You need to refresh it.[/]");

        foreach (var line in refreshCommands)
        {
            _console.WriteLine(line);
        }
        return 0;
    }
}