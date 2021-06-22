using JavaVersionSwitcher.Adapters;
using JavaVersionSwitcher.Commands;
using JavaVersionSwitcher.Commands.config;
using JavaVersionSwitcher.Logging;
using JavaVersionSwitcher.Services;
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
            app.Configure(ConfigureApp);
            return app.Run(args);
        }

        public static void ConfigureApp(IConfigurator config)
        {
            config.SetApplicationName("dotnet jvs");
            config.AddExample(new[] { "scan", "--force" });
            config.AddExample(new[] { "switch" });

            config.AddCommand<ScanJavaInstallationsCommand>("scan")
                .WithAlias("scan-for-java")
                .WithDescription("Scan for existing java installations.");
            config.AddCommand<CheckSettingsCommand>("check")
                .WithDescription("Checks if environment is set up correctly.");
            config.AddCommand<SwitchVersionCommand>("switch")
                .WithDescription("Switch to a different Java version.");
            config.AddBranch("config", cfg =>
            {
                cfg.SetDescription("get or set configurations options.");

                cfg.AddCommand<GetConfigCommand>("get")
                    .WithDescription("get configuration options.");
                cfg.AddCommand<SetConfigCommand>("set")
                    .WithDescription("set configuration options.");
                cfg.AddCommand<ShowConfigCommand>("show")
                    .WithDescription("show possible configuration options.");
            });

#if DEBUG
            config.PropagateExceptions();
            config.ValidateExamples();
#endif
        }
        
        private static ITypeRegistrar BuildRegistrar()
        {
            var container = new Container();
            container.Register<ILogger, Logger>(Lifestyle.Singleton);

            container.Register<IConfigurationService, ConfigurationService>(Lifestyle.Singleton);
            
            container.Register<IStorageAdapter, StorageAdapter>(Lifestyle.Singleton);
            container.Register<IJavaHomeAdapter, JavaHomeAdapter>(Lifestyle.Singleton);
            container.Register<IPathAdapter, PathAdapter>(Lifestyle.Singleton);
            container.Register<IJavaInstallationsAdapter, JavaInstallationsAdapter>(Lifestyle.Singleton);
            container.Register<JavaInstallationsAdapterConfigurationProvider>(Lifestyle.Singleton);
            container.Register<IShellAdapter, ShellAdapter>(Lifestyle.Singleton);

            container.Collection.Register<IConfigurationProvider>(
                new[]
                {
                    typeof(JavaInstallationsAdapterConfigurationProvider),
                },
                Lifestyle.Singleton);

            return new SimpleInjectorRegistrar(container);
        }
    }
}