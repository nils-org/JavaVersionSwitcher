
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace JavaVersionSwitcher.Adapters
{
    /// <summary>
    /// Provides access to the JAVA_HOME variable.
    /// </summary>
    public interface IJavaHomeAdapter
    {
        /// <summary>
        /// Gets the value of <c>JAVA_HOME</c>.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <returns>The value of <c>JAVA_HOME</c>.</returns>
        [NotNull] Task<string> GetValue(EnvironmentScope scope);

        /// <summary>
        /// Sets the value of <c>JAVA_HOME</c>.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <param name="scope">The scope.</param>
        Task SetValue([NotNull] string value, EnvironmentScope scope);
    }
}