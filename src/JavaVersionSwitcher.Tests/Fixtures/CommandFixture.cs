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
        public TestConsole Console => new TestConsole();
        
        public Logger Logger => new Logger();
        
        public TestConfigurationService ConfigurationService => new TestConfigurationService();
        
        public Mock<IJavaHomeAdapter> JavaHomeAdapter => new Mock<IJavaHomeAdapter>();
        
        public Mock<IPathAdapter> PathAdapter => new Mock<IPathAdapter>();
        
        public Mock<IJavaInstallationsAdapter> JavaInstallationsAdapter => new Mock<IJavaInstallationsAdapter>();
        
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