using JavaVersionSwitcher.Commands;
using JetBrains.Annotations;
using Spectre.Console.Cli;

namespace JavaVersionSwitcher
{
    [UsedImplicitly]
    public class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandApp();
            app.Configure(config =>
            {
                config.SetApplicationName("dotnet jvs");
                config.AddExample(new []{ "scan", "--force"});
                config.AddExample(new []{ "switch" });
                
                config.AddCommand<ScanJavaInstallationsCommand>("scan")
                    .WithAlias("scan-for-java")
                    .WithDescription("Scan for existing java installations.");
                config.AddCommand<CheckSettingsCommand>("check")
                    .WithDescription("Checks if environment is set up correctly.");
                config.AddCommand<SwitchVersionCommand>("switch")
                    .WithDescription("Switch to a different Java version.");
            });
            
            return app.Run(args);
        }
    }
}
