using System.Threading.Tasks;
using JavaVersionSwitcher.Logging;

namespace JavaVersionSwitcher.Adapters;

/// <inheritdoc cref="IJavaHomeAdapter"/>
public class JavaHomeAdapter : IJavaHomeAdapter
{
    private readonly ILogger _logger;
    private readonly SimpleEnvironmentAdapter _adapter;

    public JavaHomeAdapter(ILogger logger)
    {
        _logger = logger;
        _adapter = new SimpleEnvironmentAdapter("JAVA_HOME");
    }

    /// <inheritdoc cref="IJavaHomeAdapter.GetValue"/>
    public async Task<string> GetValue(EnvironmentScope scope)
    {
        var val =await _adapter.GetValue(scope);
        _logger.LogVerbose($"Read JAVA_HOME: {val}");
        return val;
    }

    /// <inheritdoc cref="IJavaHomeAdapter.SetValue"/>
    public async Task SetValue(string value, EnvironmentScope scope)
    {
        _logger.LogVerbose($"Setting JAVA_HOME to: {value}");
        await _adapter.SetValue(value, scope);
    }
}