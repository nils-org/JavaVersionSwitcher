using System.Collections.Generic;

namespace JavaVersionSwitcher.Services
{
    /// <summary>
    /// Provides some configurations
    /// </summary>
    public interface IConfigurationProvider
    {
        /// <summary>
        /// Gets the Name of this provider. 
        /// </summary>
        string ProviderName { get; }
        
        /// <summary>
        /// Gets all settings this provider provides.
        /// </summary>
        IEnumerable<string> Settings { get; } 
    }
}