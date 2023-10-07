using System.Threading.Tasks;

namespace JavaVersionSwitcher.Services;

/// <summary>
/// Access configuration settings.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Get a configuration setting.
    /// </summary>
    /// <param name="provider">The name of the provider, see <see cref="IConfigurationProvider.ProviderName"/>.</param>
    /// <param name="settingName">The name of the setting of that provider. See <see cref="IConfigurationProvider.Settings"/></param>
    /// <returns>The value.</returns>
    public Task<string> GetConfiguration(string provider, string settingName);
        
    /// <summary>
    /// Set a configuration setting.
    /// </summary>
    /// <param name="provider">The name of the provider, see <see cref="IConfigurationProvider.ProviderName"/>.</param>
    /// <param name="settingName">The name of the setting of that provider. See <see cref="IConfigurationProvider.Settings"/></param>
    /// <param name="value">The value to set.</param>
    public Task SetConfiguration(string provider, string settingName, string value);
}