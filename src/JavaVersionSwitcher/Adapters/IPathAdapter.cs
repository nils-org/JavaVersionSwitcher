using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace JavaVersionSwitcher.Adapters
{
    /// <summary>
    /// Provides access to the PATH variable
    /// and some functions for modifying it.
    /// </summary>
    public interface IPathAdapter
    {
        /// <summary>
        /// Gets the value of <c>PATH</c>.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <returns>The value of <c>JAVA_HOME</c>.</returns>
        [NotNull] Task<IEnumerable<string>> GetValue(EnvironmentScope scope);

        /// <summary>
        /// Sets the value of <c>PATH</c>.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <param name="scope">The scope.</param>
        Task SetValue([NotNull] IEnumerable<string> value, EnvironmentScope scope);
    }
}