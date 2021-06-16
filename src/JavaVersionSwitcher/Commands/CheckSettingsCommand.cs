using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JavaVersionSwitcher.Worker;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace JavaVersionSwitcher.Commands
{
    [UsedImplicitly]
    internal sealed class CheckSettingsCommand : Command<CheckSettingsCommand.Settings>
    {
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

        public override int Execute(CommandContext context, Settings settings)
        {
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (string.IsNullOrEmpty(javaHome))
            {
                AnsiConsole.MarkupLine("[red]JAVA_HOME is not set[/] JAVA_HOME needs to be set, for PATH check to work.");
                return 1;
            }

            IEnumerable<JavaInstallationScanner.JavaInstallation> javaInstallations = null;
            AnsiConsole.Status()
                .Start("Initializing...", ctx => 
                {
                    ctx.Status("Scanning");
                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("green"));

                    javaInstallations = new JavaInstallationScanner().Scan()
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                });
            
            var errors = false;
            var paths = Environment.ExpandEnvironmentVariables("%PATH%")
                .Split(";", StringSplitOptions.RemoveEmptyEntries);
            
            AnsiConsole.MarkupLine("[green]JAVA_HOME[/]: "+javaHome);
            var javaHomeBin = paths
                .FirstOrDefault(x => x.Equals(javaHome + "bin")
                                     || x.Equals(javaHome + Path.DirectorySeparatorChar + "bin"));
            if (javaHomeBin == null)
            {
                errors = true;
                AnsiConsole.MarkupLine("[red]JAVA_HOME\\bin is not in PATH[/] JAVA_HOME\\bin needs to be in PATH.");
            }
            else
            {
                AnsiConsole.MarkupLine("[green]JAVA_HOME is in PATH[/]: "+javaHomeBin);    
            }

            foreach (var java in javaInstallations)
            {
                var containedInPath = paths.Where(x => x.StartsWith(java.Location))
                    .Where(x => !x.Equals(javaHomeBin));

                foreach (var path in containedInPath)
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