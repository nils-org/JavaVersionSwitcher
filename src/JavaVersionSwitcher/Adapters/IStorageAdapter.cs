namespace JavaVersionSwitcher.Adapters
{
    /// <summary>
    /// Information about where (in the FileSystem)
    /// "our" files are located. 
    /// </summary>
    public interface IStorageAdapter
    {
        /// <summary>
        /// Gets the path to the main config file
        /// </summary>
        string ConfigurationFilePath { get; }
        
        /// <summary>
        /// Gets the path to the cache file
        /// </summary>
        string JavaInstallationCacheFilePath { get; }
    }
}