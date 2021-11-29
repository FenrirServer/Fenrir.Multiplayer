namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Payload object that is being delivered via server info endpoint,
    /// or via matchmaking / discovery
    /// </summary>
    public class ServerInfo
    {
        /// <summary>
        /// Public hostname of the server
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Unique ID of the server
        /// </summary>
        public string ServerId { get; set; }

        /// <summary>
        /// List of protocols supported by this server
        /// </summary>
        public ProtocolInfo[] Protocols { get; set; } 
    }
}
