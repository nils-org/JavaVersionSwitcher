using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using JavaVersionSwitcher.Adapters;
using JavaVersionSwitcher.Services;
using JetBrains.Annotations;
using Moq;
using Shouldly;

namespace JavaVersionSwitcher.Tests.Fixtures
{
    public class ConfigurationServiceFixture : IDisposable
    {
        private readonly List<IConfigurationProvider> _configurationProviders = new List<IConfigurationProvider>();
        private readonly Mock<IStorageAdapter> _storageAdapter;
        private readonly string _tmpFile;

        public ConfigurationServiceFixture()
        {
            _tmpFile = Path.GetTempFileName()+".xml";
            _storageAdapter = new Mock<IStorageAdapter>();
            _storageAdapter.Setup(x => x.ConfigurationFilePath).Returns(_tmpFile);
        }

        public ConfigurationService Service => new ConfigurationService(_configurationProviders, _storageAdapter.Object);

        public void WithConfigurationProvider(string providerName, params string[] settings)
        {
            var configurationProvider = new Mock<IConfigurationProvider>();
            configurationProvider.Setup(x => x.ProviderName).Returns(providerName);
            configurationProvider.Setup(x => x.Settings).Returns(settings);
            
            _configurationProviders.Add(configurationProvider.Object);
        }
        
        public void EnsureSetting([NotNull]string providerName, [NotNull]string setting, string value)
        {
            var doc = new XDocument();
            doc.Add(ReadXml());
            var root = doc.Root;
            
            var providerElm = root!.Elements(providerName).SingleOrDefault();
            if (providerElm == null)
            {
                providerElm = new XElement(providerName);
                root.Add(providerElm);
            }
            
            var settingElm = providerElm.Elements(setting).SingleOrDefault();
            if (settingElm == null)
            {
                settingElm = new XElement(setting);
                providerElm.Add(settingElm);
            }

            settingElm.Value = value;
            doc.Save(_tmpFile);
        }

        public XElement ReadXml(string providerName = null, string setting = null)
        {
            if (!File.Exists(_tmpFile))
            {
                return new XElement("temp-settings");
            }
            
            var xml = XDocument.Load(_tmpFile);
            if (providerName == null)
            {
                return xml.Root;
            }
            
            var providerElm = xml.Root!.Elements(providerName).SingleOrDefault();
            providerElm.ShouldNotBeNull("a provider element should have been created.");
            var settingElm = providerElm.Elements(setting).SingleOrDefault();
            if (setting == null)
            {
                return providerElm;
            }
            
            settingElm.ShouldNotBeNull("a settings element should have been created.");
            return settingElm;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            if (_tmpFile != null && File.Exists(_tmpFile))
            {
                File.Delete(_tmpFile);
            }
        }
    }
}