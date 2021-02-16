using System;

namespace Fenrir.Multiplayer.Server
{
    /// <summary>
    /// Server Info Service
    /// Starts Http Server that returns information about server,
    /// available protocols, encryption keys etc
    /// </summary>
    interface IServerInfoService : IDisposable
    {
        /// <summary>
        /// Indicates if service is running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Port on which HTTP server listens.
        /// 8080 by default
        /// </summary>
        ushort Port { get; }

        /// <summary>
        /// Starts the service.
        /// This method is invoked when NetworkServer is starting, before any protocols are initialized
        /// </summary>
        /// <param name="bindPort">Port on which service listens</param>
        /// <returns>Task that must complete when service has started. Failing this task will fail <see cref="NetworkServer.Start"/></returns>
        void Start(ushort bindPort);

        /// <summary>
        /// Stops the service.
        /// This method is invoked when NetworkServer is stopping, after any protocols are stopped.
        /// </summary>
        /// <returns>Task that must complete when service has stopped. Failing this task will fail <see cref="NetworkServer.Stop"/></returns>
        void Stop();
    }
}