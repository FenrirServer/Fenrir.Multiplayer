using System;

namespace Fenrir.Multiplayer.Server
{
    /// <summary>
    /// Server Info Service
    /// Starts Http Server that returns information about server,
    /// available protocols, encryption keys etc
    /// </summary>
    interface IServerInfoService : IService, IDisposable
    {
        /// <summary>
        /// Port on which HTTP server listens.
        /// 8080 by default
        /// </summary>
        ushort Port { get; set; }
    }
}