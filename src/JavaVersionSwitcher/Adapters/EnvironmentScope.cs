namespace JavaVersionSwitcher.Adapters;

/// <summary>
/// Defines the scope of environment access. 
/// </summary>
public enum EnvironmentScope
{
    /// <summary>
    /// Current process scope.
    /// <para>
    /// Reading this value means accessing the "effective"
    /// value. I.e. Process > User > Machine  
    /// </para>
    /// </summary>
    Process,
        
    /// <summary>
    /// User scope.
    /// </summary>
    User,
        
    /// <summary>
    /// Machine scope.
    /// <para>
    /// (Writing this scope might require super-user permissions.)
    /// </para>
    /// </summary>
    Machine
}