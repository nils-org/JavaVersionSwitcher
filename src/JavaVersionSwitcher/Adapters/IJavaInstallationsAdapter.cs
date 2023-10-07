using System.Collections.Generic;
using System.Threading.Tasks;
using JavaVersionSwitcher.Models;

namespace JavaVersionSwitcher.Adapters;

/// <summary>
/// Provides access to java installations .
/// </summary>
public interface IJavaInstallationsAdapter
{
    /// <summary>
    /// List all installed java versions.
    /// </summary>
    /// <param name="forceReScan"></param>
    /// <returns>The list java installations.</returns>
    Task<IEnumerable<JavaInstallation>> GetJavaInstallations(bool forceReScan = false);
}