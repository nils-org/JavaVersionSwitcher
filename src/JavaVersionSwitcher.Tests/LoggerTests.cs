using JavaVersionSwitcher.Logging;
using Shouldly;
using Spectre.Console.Testing;
using Xunit;

namespace JavaVersionSwitcher.Tests
{
    public class LoggerTests
    {
        private readonly TestConsole _console;
        private readonly ILogger _logger;

        public LoggerTests()
        {
            _console = new TestConsole();
            _logger = new Logger(_console);
        }
        
        [Fact]
        public void Writes_warning_with_prefix()
        {
            _logger.LogWarning("test");

            _console.Output.ShouldStartWith("WARNING:");
        }
        
        [Fact]
        public void Writes_verbose_when_verbose_is_set()
        {
            _logger.PrintVerbose = true;
            _logger.LogVerbose("test");

            _console.Output.ShouldBe("test\n");
        }
        
        [Fact]
        public void Writes_nothing_when_verbose_is_not_set()
        {
            _logger.PrintVerbose = false;
            _logger.LogVerbose("test");

            _console.Output.ShouldBe(string.Empty);
        }
    }
}