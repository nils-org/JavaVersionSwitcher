using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JavaVersionSwitcher.Adapters;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace JavaVersionSwitcher.Commands
{
    [UsedImplicitly]
    internal sealed class CheckSettingsCommand : AsyncCommand<CheckSettingsCommand.Settings>
    {
        private readonly IJavaHomeAdapter _javaHomeAdapter;
        private readonly IJavaInstallationsAdapter _javaInstallationsAdapter;
        private readonly IPathAdapter _pathAdapter;

        public CheckSettingsCommand(
            IJavaHomeAdapter javaHomeAdapter,
            IJavaInstallationsAdapter javaInstallationsAdapter,
            IPathAdapter pathAdapter
        )
        {
            _javaHomeAdapter = javaHomeAdapter;
            _javaInstallationsAdapter = javaInstallationsAdapter;
            _pathAdapter = pathAdapter;
        }
        
        [UsedImplicitly]
        public sealed class Settings : CommandSettings
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
            var javaHome = await _javaHomeAdapter.GetValue(EnvironmentScope.Process);
            if (string.IsNullOrEmpty(javaHome))
            {
                AnsiConsole.MarkupLine("[red]JAVA_HOME is not set[/] JAVA_HOME needs to be set, for PATH check to work.");
                return 1;
            }
            AnsiConsole.MarkupLine("[green]JAVA_HOME[/]: "+javaHome);

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
                AnsiConsole.MarkupLine("[green]JAVA_HOME\\bin is in PATH[/]: "+javaHomeInPath);    
            }
            else
            {
                errors = true;
                AnsiConsole.MarkupLine("[red]JAVA_HOME\\bin is not in PATH[/] JAVA_HOME\\bin needs to be in PATH.");
            }

            foreach (var java in javaInstallations)
            {
                var javaInstallationsInPath = paths
                    .Where(x => x.StartsWith(java.Location, StringComparison.OrdinalIgnoreCase))
                    .Where(x => !x.Equals(javaHomeInPath));

                foreach (var path in javaInstallationsInPath)
                {
                    errors = true;
                    AnsiConsole.MarkupLine($"[red]Additional java in PATH[/]: {path}");    
                }
            }
            
            if (errors)
            {
                AnsiConsole.MarkupLine("[yellow]There were errors.[/]");
                AnsiConsole.MarkupLine("[yellow]Fixing errors is not implemented. You need to fix them manually![/]");
                return 1;
            }

            return 0;
        }
    }
}