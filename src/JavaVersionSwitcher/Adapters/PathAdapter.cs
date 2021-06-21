using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace JavaVersionSwitcher.Adapters
{
    /// <inheritdoc cref="IPathAdapter"/>
    public class PathAdapter : IPathAdapter
    {
        private readonly SimpleEnvironmentAdapter _adapter;

        public PathAdapter()
        {
            _adapter = new SimpleEnvironmentAdapter("PATH");
        }

        /// <inheritdoc cref="IPathAdapter.GetValue"/>
        public async Task<IEnumerable<string>> GetValue(EnvironmentScope scope)
        {
            var fullPath = await _adapter.GetValue(scope);
            return fullPath.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <inheritdoc cref="IPathAdapter.SetValue"/>
        public async Task SetValue(IEnumerable<string> value, EnvironmentScope scope)
        {
            var fullPath = string.Join(Path.PathSeparator, value);
            await _adapter.SetValue(fullPath, scope);
        }
    }
}