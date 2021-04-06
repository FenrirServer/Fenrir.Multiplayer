namespace Fenrir.Multiplayer.Logging
{
    /// <summary>
    /// Logger interface - provides ways for Fenrir libraries
    /// to log messages.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Writes Trace Log
        /// </summary>
        /// <param name="format">Message</param>
        /// <param name="arguments">Arguments</param>
        void Trace(string format, params object[] arguments);

        /// <summary>
        /// Writes Debug Log
        /// </summary>
        /// <param name="format">Message</param>
        /// <param name="arguments">Arguments</param>
        void Debug(string format, params object[] arguments);

        /// <summary>
        /// Writes Info Log
        /// </summary>
        /// <param name="format">Message</param>
        /// <param name="arguments">Arguments</param>
        void Info(string format, params object[] arguments);

        /// <summary>
        /// Writes Warning Log
        /// </summary>
        /// <param name="format">Message</param>
        /// <param name="arguments">Arguments</param>
        void Warning(string format, params object[] arguments);

        /// <summary>
        /// Writes Error Log
        /// </summary>
        /// <param name="format">Message</param>
        /// <param name="arguments">Arguments</param>
        void Error(string format, params object[] arguments);

        /// <summary>
        /// Writes Critical Log
        /// </summary>
        /// <param name="format">Message</param>
        /// <param name="arguments">Arguments</param>
        void Critical(string format, params object[] arguments);
    }
}
