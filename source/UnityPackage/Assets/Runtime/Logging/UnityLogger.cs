#if UNITY_2018_1_OR_NEWER
namespace Fenrir.Multiplayer
{
    public class UnityLogger : Fenrir.Multiplayer.ILogger
    {
        public void Critical(string format, params object[] arguments) => UnityEngine.Debug.LogErrorFormat(format, arguments);

        public void Debug(string format, params object[] arguments) => UnityEngine.Debug.LogFormat(format, arguments);

        public void Error(string format, params object[] arguments) => UnityEngine.Debug.LogErrorFormat(string.Format(format, arguments));

        public void Info(string format, params object[] arguments) => UnityEngine.Debug.LogFormat(string.Format(format, arguments));

        public void Trace(string format, params object[] arguments) => UnityEngine.Debug.LogFormat(string.Format(format, arguments));

        public void Warning(string format, params object[] arguments) => UnityEngine.Debug.LogWarningFormat(string.Format(format, arguments));
    }
}
#endif
