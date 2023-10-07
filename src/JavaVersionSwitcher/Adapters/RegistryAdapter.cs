using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using JavaVersionSwitcher.Logging;
using JetBrains.Annotations;
using Microsoft.Win32;

namespace JavaVersionSwitcher.Adapters;

public class RegistryAdapter : IRegistryAdapter
{
    private readonly ILogger _logger;

    public RegistryAdapter(ILogger logger)
    {
        _logger = logger;
    }

    [ItemCanBeNull]
    private static Task<AsyncRegistryKey> GetHklmAsync()
    {
        return AsyncRegistryKey.Hklm;
    }

    internal class AsyncRegistryKey : IDisposable
    {
#pragma warning disable CA1416
        private readonly RegistryKey _key;

        private AsyncRegistryKey(RegistryKey key)
        {
            _key = key;
        }

        [ItemCanBeNull]
        internal static Task<AsyncRegistryKey> Hklm
        {
            get
            {
                return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? Task.Factory.StartNew(() => new AsyncRegistryKey(Registry.LocalMachine))
                    : Task.Factory.StartNew<AsyncRegistryKey>(() => null);
            }
        }

        [ItemCanBeNull]
        internal Task<AsyncRegistryKey> OpenSubKey(string name)
        {
            return Task.Factory.StartNew(() =>
            {
                var subKey = _key.OpenSubKey(name);
                return subKey == null 
                    ? null 
                    : new AsyncRegistryKey(subKey);
            });
        }

        internal Task<string[]> GetSubKeyNames()
        {
            return Task.FromResult(_key.GetSubKeyNames());
        }
        
        [ItemCanBeNull]
        internal Task<object> GetValue(string name)
        {
            return Task.FromResult(_key.GetValue(name));
        }

        public override string ToString()
        {
            return _key.ToString();
        }

        public void Dispose()
        {
            _key?.Dispose();
        }
#pragma warning restore CA1416
    }

    public async IAsyncEnumerable<IJavaRegistryRegistration> GetInstallations()
    {
        using var localMachine = await GetHklmAsync();
        if (localMachine == null)
        {
            _logger.LogVerbose($"Not Checking the registry, since we're running on {RuntimeInformation.RuntimeIdentifier}");
            yield break;
        }

        using var javaSoft = await localMachine.OpenSubKey("SOFTWARE\\JavaSoft");
        if (javaSoft == null)
        {
            _logger.LogWarning(@"RegKey 'HKLM\Software\JavaSoft' does not exist.");
            yield break;
        }
        
        var roots = new[]
        {
            "Java Development Kit",
            "Java Development Kit",
            "JDK",
            "JRE",
        };

        foreach (var root in roots)
        {
            using var rootKey = await javaSoft.OpenSubKey(root);
            if (rootKey == null)
            {
                continue;
            }

            var versions = await rootKey.GetSubKeyNames();
            foreach (var javaVersion in versions)
            {
                _logger.LogVerbose($"Checking SubKey: {rootKey}\\{javaVersion}");
                using var key = await rootKey.OpenSubKey(javaVersion);
                if (key == null)
                {
                    _logger.LogWarning($"SubKey '{rootKey}\\{javaVersion}' was reported to exist, but it does not.");
                    continue;
                }
                
                if (await key.GetValue("JavaHome") is not string javaHome)
                {
                    continue;
                }

                yield return new JavaRegistryRegistration
                {
                    RegKey = key.ToString(),
                    InstallationPath = javaHome,
                    Version = javaVersion,
                };
            }
        }
    }

    private class JavaRegistryRegistration : IJavaRegistryRegistration
    {
        public string RegKey { get; init; }
        public string InstallationPath { get; init; }
        public string Version { get; init; }
    }
}