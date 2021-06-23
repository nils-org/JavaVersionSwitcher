using System;
using System.IO;

namespace JavaVersionSwitcher.Adapters
{
    /// <inheritdoc cref="IStorageAdapter"/>
    public class StorageAdapter : IStorageAdapter
    {
        private string _baseFolder;

        /// <inheritdoc cref="IStorageAdapter.ConfigurationFilePath"/>
        public string ConfigurationFilePath => GetPath("settings.xml");

        /// <inheritdoc cref="IStorageAdapter.JavaInstallationCacheFilePath"/>
        public string JavaInstallationCacheFilePath => GetPath("installations.xml");

        private string GetPath(string fileName)
        {
            if (_baseFolder == null)
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var app = typeof(StorageAdapter).Assembly.GetName().Name ?? "JavaVersionSwitcher";
                _baseFolder = Path.Combine(appData, app);
            }

            return Path.Combine(_baseFolder, fileName);
        } 
    }
}