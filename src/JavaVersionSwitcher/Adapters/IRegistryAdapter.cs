using System.Collections.Generic;

namespace JavaVersionSwitcher.Adapters;

public interface IRegistryAdapter
{
    IAsyncEnumerable<IJavaRegistryRegistration> GetInstallations();
}

public interface IJavaRegistryRegistration
{
    string RegKey { get; }
    
    string InstallationPath { get; }
    
    string Version { get; }
}