namespace Fenrir.Multiplayer.Logging
{
    public class EventBasedLogger : IFenrirLogger
    {
        public delegate void LogHandler(LogLevel level, string format, params object[] arguments);

        public event LogHandler Log;

        public void Trace(string format, params object[] arguments) => Log?.Invoke(LogLevel.Trace, format, arguments);

        public void Debug(string format, params object[] arguments) => Log?.Invoke(LogLevel.Debug, format, arguments);

        public void Info(string format, params object[] arguments) => Log?.Invoke(LogLevel.Info, format, arguments);

        public void Warning(string format, params object[] arguments) => Log?.Invoke(LogLevel.Warning, format, arguments);

        public void Error(string format, params object[] arguments) => Log?.Invoke(LogLevel.Error, format, arguments);

        public void Critical(string format, params object[] arguments) => Log?.Invoke(LogLevel.Critical, format, arguments);
    }
}
