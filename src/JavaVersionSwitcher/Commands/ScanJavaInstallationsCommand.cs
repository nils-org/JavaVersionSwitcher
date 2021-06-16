using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JavaVersionSwitcher.Worker;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace JavaVersionSwitcher.Commands
{
    [UsedImplicitly]
    internal sealed class ScanJavaInstallationsCommand : Command<ScanJavaInstallationsCommand.Settings>
    {
        [UsedImplicitly]
        public sealed class Settings : CommandSettings
        {
            [CommandOption("--force")]
            [DefaultValue(false)]
            [Description("Force re-scan and do not use cached data.")]
            public bool Force { get; [UsedImplicitly] set; }
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

                    installations = worker.Scan(settings.Force)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                });

            var table = new Table();
            table.AddColumn("Java path");
            table.AddColumn("version");
            foreach (var javaInstallation in installations.OrderBy(x => x.Location))
            {
                //var txt = javaInstallation.Version;
                //if(ver)
                table.AddRow(javaInstallation.Location, javaInstallation.Version ?? "unknown");
            }

            AnsiConsole.Render(table);
            return 0;
        }
    }
}