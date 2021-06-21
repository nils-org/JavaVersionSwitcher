using System.ComponentModel;
using Spectre.Console.Cli;

namespace JavaVersionSwitcher.Commands
{
    public abstract class CommonCommandSettings : CommandSettings
    {
        [CommandOption("--verbose")]
        [Description("Show verbose messages.")]
        [DefaultValue(false)]
        public bool Verbose { get; set; }
    }
}