using System;

namespace Fenrir.Multiplayer.Server.Events
{
    /// <summary>
    /// Contains event arguments invoked when server status changes
    /// </summary>
    public class ServerStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// New server status
        /// </summary>
        public ServerStatus Status { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="status">Server status</param>
        public ServerStatusChangedEventArgs(ServerStatus status)
        {
            Status = status;
        }
    }
}
