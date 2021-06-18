using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JavaVersionSwitcher.Adapters;
using JavaVersionSwitcher.Logging;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace JavaVersionSwitcher.Commands
{
    [UsedImplicitly]
    internal sealed class SwitchVersionCommand : AsyncCommand<SwitchVersionCommand.Settings>
    {
        private readonly IJavaHomeAdapter _javaHomeAdapter;
        private readonly IJavaInstallationsAdapter _javaInstallationsAdapter;
        private readonly IPathAdapter _pathAdapter;
        private readonly ILogger _logger;

        public SwitchVersionCommand(
            IJavaHomeAdapter javaHomeAdapter,
            IJavaInstallationsAdapter javaInstallationsAdapter,
            IPathAdapter pathAdapter,
            ILogger logger
        )
        {
            _javaHomeAdapter = javaHomeAdapter;
            _javaInstallationsAdapter = javaInstallationsAdapter;
            _pathAdapter = pathAdapter;
            _logger = logger;
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

            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Which java should be set?")
                    .PageSize(25)
                    .MoreChoicesText("[grey](Move up and down to reveal more installations)[/]")
                    .AddChoices(installations.Select(x => x.Location).ToArray())
            );

            await AnsiConsole.Status()
                .StartAsync("Applying...", async ctx =>
                {
                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("green"));

                    var scope = settings.MachineScope
                        ? EnvironmentScope.Machine
                        : EnvironmentScope.User;

                    var javaHome = await _javaHomeAdapter.GetValue(EnvironmentScope.Process);
                    var paths = (await _pathAdapter.GetValue(scope)).ToList();
                    if (!string.IsNullOrEmpty(javaHome))
                    {
                        paths = paths.Where(x => !x.StartsWith(javaHome,StringComparison.OrdinalIgnoreCase)).ToList();
                    }

                    paths.Add(Path.Combine(selected, "bin"));

                    await _javaHomeAdapter.SetValue(selected, scope);
                    await _pathAdapter.SetValue(paths, scope);
                }).ConfigureAwait(false);

            AnsiConsole.MarkupLine("[yellow]The environment has been modified. You need to refresh it.[/]");
            return 0;
        }
    }
}