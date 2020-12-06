using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Server
{
    /// <summary>
    /// Server Info Service
    /// Starts Http Server that returns information about server,
    /// available protocols, encryption keys etc
    /// </summary>
    public interface IServerInfoService
    {
        /// <summary>
        /// Port on which HTTP server listens.
        /// 8080 by default
        /// </summary>
        short Port { get; set; }

        /// <summary>
        /// Indicates if service is running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Starts a web serverr
        /// </summary>
        /// <returns>Task that completes when service is running</returns>
        Task Start();

        /// <summary>
        /// Stops the web server
        /// </summary>
        /// <returns>Task that completes when web server has stopped</returns>
        Task Stop();
    }
}