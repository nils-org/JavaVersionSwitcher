using System.ComponentModel;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace JavaVersionSwitcher.Commands.config
{
    public abstract class CommonConfigCommandSettings : CommonCommandSettings
    {
        [CommandArgument(0, "[Provider]")]
        [Description("Name of the provider.")]
        public string Provider { get; [UsedImplicitly] set; }

        [CommandArgument(1, "[Name]")]
        [Description("Name of the option for that provider.")]
        public string Name { get; [UsedImplicitly] set; }
        
        public override ValidationResult Validate()
        {
            if (string.IsNullOrEmpty(Provider))
            {
                return ValidationResult.Error("Provider must be set.");
            }

            if (string.IsNullOrEmpty(Name))
            {
                return ValidationResult.Error("Name must be set.");
            }

            return ValidationResult.Success();
        }
    }
}