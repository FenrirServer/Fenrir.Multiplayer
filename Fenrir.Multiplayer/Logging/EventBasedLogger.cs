namespace Fenrir.Multiplayer.Logging
{
    /// <summary>
    /// Event-based implementation for Logger.
    /// Invokes an event when logged
    /// </summary>
    public class EventBasedLogger : ILogger
    {
        /// <summary>
        /// Log Handler - invoked when message is logged
        /// </summary>
        /// <param name="level">Log Level</param>
        /// <param name="format">Message</param>
        /// <param name="arguments">Arguments</param>
        public delegate void LogHandler(LogLevel level, string format, params object[] arguments);

        /// <summary>
        /// Log Event
        /// </summary>
        public event LogHandler Logged;

        /// <inheritdoc/>
        public void Trace(string format, params object[] arguments) => Logged?.Invoke(LogLevel.Trace, format, arguments);

        /// <inheritdoc/>
        public void Debug(string format, params object[] arguments) => Logged?.Invoke(LogLevel.Debug, format, arguments);

        /// <inheritdoc/>
        public void Info(string format, params object[] arguments) => Logged?.Invoke(LogLevel.Info, format, arguments);

        /// <inheritdoc/>
        public void Warning(string format, params object[] arguments) => Logged?.Invoke(LogLevel.Warning, format, arguments);

        /// <inheritdoc/>
        public void Error(string format, params object[] arguments) => Logged?.Invoke(LogLevel.Error, format, arguments);

        /// <inheritdoc/>
        public void Critical(string format, params object[] arguments) => Logged?.Invoke(LogLevel.Critical, format, arguments);
    }
}
