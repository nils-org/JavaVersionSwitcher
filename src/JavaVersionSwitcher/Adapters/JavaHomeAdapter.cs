using System.Threading.Tasks;

namespace JavaVersionSwitcher.Adapters
{
    /// <inheritdoc cref="IJavaHomeAdapter"/>
    public class JavaHomeAdapter : IJavaHomeAdapter
    {
        private readonly SimpleEnvironmentAdapter _adapter;

        public JavaHomeAdapter()
        {
            _adapter = new SimpleEnvironmentAdapter("JAVA_HOME");
        }

        /// <inheritdoc cref="IJavaHomeAdapter.GetValue"/>
        public async Task<string> GetValue(EnvironmentScope scope)
        {
            return await _adapter.GetValue(scope);
        }

        /// <inheritdoc cref="IJavaHomeAdapter.SetValue"/>
        public async Task SetValue(string value, EnvironmentScope scope)
        {
            await _adapter.SetValue(value, scope);
        }
    }
}