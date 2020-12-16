namespace Fenrir.Multiplayer.Logging
{
    /// <summary>
    /// Indicates level / severity of log message
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Trace logs - used for tracing (method start / end). 
        /// Should be disable in production.
        /// </summary>
        Trace = 0,

        /// <summary>
        /// Debug logs - network events. 
        /// Should be disable in production.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Informational logs
        /// </summary>
        Info = 2,

        /// <summary>
        /// Warnings
        /// </summary>
        Warning = 3,

        /// <summary>
        /// Errors
        /// </summary>
        Error = 4,

        /// <summary>
        /// Critical errors
        /// </summary>
        Critical = 5
    }
}
