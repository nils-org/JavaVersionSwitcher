using System;
using System.Threading.Tasks;

namespace JavaVersionSwitcher.Adapters;

/// <summary>
/// Provides generic access to an environment variable.
/// </summary>
internal class SimpleEnvironmentAdapter
{
    private readonly string _variableName;

    internal SimpleEnvironmentAdapter(string variableName)
    {
        _variableName = variableName;
    }
        
    public Task<string> GetValue(EnvironmentScope scope)
    {
        return Task.Factory.StartNew(() =>
        {
            var target = Convert(scope);
            return Environment.GetEnvironmentVariable(_variableName, target) ?? "";
        });
    }
        
    public Task SetValue(string value, EnvironmentScope scope)
    {
        return Task.Factory.StartNew(() =>
        {
            var target = Convert(scope);
            Environment.SetEnvironmentVariable(_variableName, value, target);
        });
    }

    private static EnvironmentVariableTarget Convert(EnvironmentScope scope)
    {
        var target = scope switch
        {
            EnvironmentScope.Machine => EnvironmentVariableTarget.Machine,
            EnvironmentScope.User => EnvironmentVariableTarget.User,
            EnvironmentScope.Process => EnvironmentVariableTarget.Process,
            _ => EnvironmentVariableTarget.Process
        };

        return target;
    }
}