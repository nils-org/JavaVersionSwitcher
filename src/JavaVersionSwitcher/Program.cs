using JavaVersionSwitcher.Adapters;
using JavaVersionSwitcher.Commands;
using JetBrains.Annotations;
using SimpleInjector;
using Spectre.Console.Cli;

namespace JavaVersionSwitcher
{
    [UsedImplicitly]
    public class Program
    {
        private static int Main(string[] args)
        {
            var registrar = BuildRegistrar();
            var app = new CommandApp(registrar);
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

                config.ValidateExamples();
            });
            
            return app.Run(args);
        }

        private static ITypeRegistrar BuildRegistrar()
        {
            var container = new Container();
            container.Register<IJavaHomeAdapter, JavaHomeAdapter>(Lifestyle.Singleton);
            container.Register<IPathAdapter, PathAdapter>(Lifestyle.Singleton);
            container.Register<IJavaInstallationsAdapter, JavaInstallationsAdapter>(Lifestyle.Singleton);
            
            return new SimpleInjectorRegistrar(container);
        }
    }
}
