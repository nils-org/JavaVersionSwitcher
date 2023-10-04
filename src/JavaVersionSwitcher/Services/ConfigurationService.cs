using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using JavaVersionSwitcher.Adapters;

namespace JavaVersionSwitcher.Services;

/// <inheritdoc cref="IConfigurationService"/>
public class ConfigurationService : IConfigurationService
{
    private readonly IStorageAdapter _storageAdapter;
    private readonly IReadOnlyCollection<IConfigurationProvider> _providers;
    private readonly XNamespace _ns = XNamespace.None;

    public ConfigurationService(
        IEnumerable<IConfigurationProvider> providers,
        IStorageAdapter storageAdapter)
    {
        _storageAdapter = storageAdapter;
        _providers = providers?.ToList();
    }
        
    /// <inheritdoc cref="IConfigurationService.GetConfiguration"/>
    public async Task<string> GetConfiguration(string providerName, string settingName)
    {
        var provider = GetProvider(providerName);
        EnsureSettingNameIsValid(provider, settingName);

        return await ReadSetting(provider.ProviderName, settingName);
    }

    /// <inheritdoc cref="IConfigurationService.SetConfiguration"/>
    public async Task SetConfiguration(string providerName, string settingName, string value)
    {
        var provider = GetProvider(providerName);
        EnsureSettingNameIsValid(provider, settingName);

        await WriteSetting(provider.ProviderName, settingName, value);
    }

    private async Task WriteSetting(string providerName, string settingName, string value)
    {
        var doc = await GetXmlConfig();
        var root = doc.Root;
        var providerElement = root!.Elements(_ns + providerName).FirstOrDefault();
        if (providerElement == null)
        {
            providerElement = new XElement(_ns + providerName);
            root.Add(providerElement);
        }
            
        var settingElement = providerElement.Elements(_ns + settingName).FirstOrDefault();
        if (settingElement == null)
        {
            settingElement = new XElement(_ns + settingName);
            providerElement.Add(settingElement);
        }

        if (string.IsNullOrEmpty(value))
        {
            settingElement.Remove();
        }
        else
        {
            settingElement.Value = value;    
        }

        if (!providerElement.Elements().Any())
        {
            providerElement.Remove();
        }
            
        await SaveXmlConfig(doc);
    }

    private async Task SaveXmlConfig(XDocument doc)
    {
        var fileName = _storageAdapter.ConfigurationFilePath;
        await using var stream = new StreamWriter(fileName, false, Encoding.UTF8);
        await doc.SaveAsync(stream, SaveOptions.None, CancellationToken.None);
    }

    private async Task<string> ReadSetting(string providerName, string settingName)
    {
        var doc = await GetXmlConfig();
        var providerElement = doc.Root!.Elements(_ns + providerName).FirstOrDefault();
        if (providerElement == null)
        {
            return string.Empty;
        }
            
        var settingElement = providerElement.Elements(_ns + settingName).FirstOrDefault();
        return settingElement?.Value ?? string.Empty;
    }

    private async Task<XDocument> GetXmlConfig()
    {
        var fileName = _storageAdapter.ConfigurationFilePath;
        var file = new FileInfo(fileName);
        if (!file.Exists)
        {
            var doc = new XDocument();
            doc.Add(new XElement(_ns + "JavaVersionSwitcher"));
            return doc;
        }

        await using var stream = file.OpenRead();
        var loaded = XDocument.Load(stream);
        return loaded;
    }

    private void EnsureSettingNameIsValid(IConfigurationProvider provider, string name)
    {
        var exists = provider.Settings.Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (!exists)
        {
            throw new KeyNotFoundException(
                $"No Configuration with the name of {name} exists in provider {provider.ProviderName}.");
        }
    }
        
    private IConfigurationProvider GetProvider(string name)
    {
        return _providers.FirstOrDefault(x =>
                   x.ProviderName.Equals(name, StringComparison.OrdinalIgnoreCase)) ??
               throw new KeyNotFoundException($"No ConfigurationProvider with the name of {name} exists.");
    }
}