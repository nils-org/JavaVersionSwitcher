using JavaVersionSwitcher.Adapters;
using JavaVersionSwitcher.Logging;
using JavaVersionSwitcher.Services;
using JavaVersionSwitcher.Tests.TestImplementations;
using Moq;
using SimpleInjector;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace JavaVersionSwitcher.Tests.Fixtures
{
    public class CommandFixture
    {
        public CommandFixture()
        {
            Logger = new Logger(Console);
        }
        
        public TestConsole Console { get; } = new TestConsole();
        
        public Logger Logger { get; }
        
        public TestConfigurationService ConfigurationService { get; } = new TestConfigurationService();
        
        public Mock<IJavaHomeAdapter> JavaHomeAdapter { get; } = new Mock<IJavaHomeAdapter>();
        
        public Mock<IPathAdapter> PathAdapter { get; } = new Mock<IPathAdapter>();
        
        public Mock<IJavaInstallationsAdapter> JavaInstallationsAdapter { get; } = new Mock<IJavaInstallationsAdapter>();
        
        private ITypeRegistrar BuildRegistrar()
        {
            var container = new Container();
            container.RegisterInstance<ILogger>(Logger);
            container.RegisterInstance<IConfigurationService>(ConfigurationService);
            container.RegisterInstance(JavaHomeAdapter.Object);
            container.RegisterInstance(PathAdapter.Object);
            container.RegisterInstance(JavaInstallationsAdapter.Object);

            container.Register<JavaInstallationsAdapterConfigurationProvider>(Lifestyle.Singleton);

            container.Collection.Register<IConfigurationProvider>(
                new[]
                {
                    typeof(JavaInstallationsAdapterConfigurationProvider),
                },
                Lifestyle.Singleton);

            return new SimpleInjectorRegistrar(container);
        }

        public int Run(params string[] args)
        {
            AnsiConsole.Console = Console;
            var registrar = BuildRegistrar();
            var app = new CommandApp(registrar);
            app.Configure(Program.ConfigureApp);
            
            return app.Run(args);
        }
    }
}