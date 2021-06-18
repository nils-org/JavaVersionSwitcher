using Spectre.Console;

namespace JavaVersionSwitcher.Logging
{
    public class Logger : ILogger
    {
        public bool PrintVerbose { get; set; }
        
        public void LogVerbose(string text)
        {
            if (!PrintVerbose)
            {
                return;
            }
            
            AnsiConsole.MarkupLine($"[gray]{text}[/]");
        }
    }
}