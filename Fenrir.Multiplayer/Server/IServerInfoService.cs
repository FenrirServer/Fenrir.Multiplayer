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
        /// Host name of the http server.
        /// 0.0.0.0 by default
        /// </summary>
        string HostName { get; set; }

        /// <summary>
        /// Port on which HTTP server listens.
        /// 8080 by default
        /// </summary>
        short Port { get; set; }

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