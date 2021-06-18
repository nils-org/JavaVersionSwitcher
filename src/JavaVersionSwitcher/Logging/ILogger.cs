namespace JavaVersionSwitcher.Logging
{
    /// <summary>
    /// The internal logger
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Verbose messages will be printed,
        /// only if this is set to <c>true</c>
        /// </summary>
        bool PrintVerbose { get; set; }
        
        /// <summary>
        /// Log a verbose message.
        /// See also <see cref="PrintVerbose"/>.
        /// </summary>
        /// <param name="text">The text to print.</param>
        void LogVerbose(string text);
    }
}