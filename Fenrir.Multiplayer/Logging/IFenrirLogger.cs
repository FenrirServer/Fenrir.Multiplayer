namespace Fenrir.Multiplayer.Logging
{
    public interface IFenrirLogger
    {
        void Trace(string format, params object[] arguments);
        void Debug(string format, params object[] arguments);
        void Info(string format, params object[] arguments);
        void Warning(string format, params object[] arguments);
        void Error(string format, params object[] arguments);
        void Critical(string format, params object[] arguments);
    }
}
