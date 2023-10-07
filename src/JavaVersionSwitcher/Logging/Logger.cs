using Spectre.Console;

namespace JavaVersionSwitcher.Logging;

public class Logger : ILogger
{
    private readonly IAnsiConsole _console;

    public Logger(IAnsiConsole console)
    {
        _console = console;
    }
        
    public bool PrintVerbose { get; set; }
        
    public void LogVerbose(string text)
    {
        if (!PrintVerbose)
        {
            return;
        }
            
        _console.MarkupLine($"[gray]{text}[/]");
    }

    public void LogWarning(string text)
    {
        _console.MarkupLine($"[yellow]WARNING: {text}[/]");
    }
}