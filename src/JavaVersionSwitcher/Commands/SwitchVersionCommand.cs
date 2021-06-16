using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using JavaVersionSwitcher.Worker;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace JavaVersionSwitcher.Commands
{
    [UsedImplicitly]
    internal sealed class SwitchVersionCommand : Command<SwitchVersionCommand.Settings>
    {
        [UsedImplicitly]
        public sealed class Settings : CommandSettings
        {
            [CommandOption("--machine")]
            [DefaultValue(false)]
            [Description("set variables in machine scope instead of user. needs admin privileges.")]
            public bool MachineScope { get; [UsedImplicitly] set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            var worker = new JavaInstallationScanner();
            IEnumerable<JavaInstallationScanner.JavaInstallation> installations = null;
            AnsiConsole.Status()
                .Start("Initializing...", ctx =>
                {
                    ctx.Status("Scanning");
                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("green"));

                    installations = worker.Scan()
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                });

            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Which java should be set?")
                    .PageSize(20)
                    .MoreChoicesText("[grey](Move up and down to reveal more installations)[/]")
                    .AddChoices(installations.Select(x => x.Location).ToArray())
            );

            AnsiConsole.Status()
                .Start("Applying...", ctx =>
                {
                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("green"));

                    var target = settings.MachineScope
                        ? EnvironmentVariableTarget.Machine
                        : EnvironmentVariableTarget.User;

                    var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME", target);
                    var paths = (Environment.GetEnvironmentVariable("PATH", target) ?? "")
                        .Split(";", StringSplitOptions.RemoveEmptyEntries);
                    var newPaths = paths.ToList();
                    if (!string.IsNullOrEmpty(javaHome))
                    {
                        newPaths = newPaths.Where(x => !x.StartsWith(javaHome)).ToList();
                    }

                    newPaths.Add(Path.Combine(selected, "bin"));

                    Environment.SetEnvironmentVariable("JAVA_HOME", selected, target);
                    Environment.SetEnvironmentVariable("PATH", string.Join(Path.PathSeparator, newPaths), target);
                });

            AnsiConsole.MarkupLine("[yellow]The environment has been modified. You need to refresh it.[/]");
            return 0;
        }
    }
}