using System.Collections.Generic;

namespace JavaVersionSwitcher.Adapters;

public interface IRegistryAdapter
{
    IAsyncEnumerable<string> GetInstallationPaths();
}