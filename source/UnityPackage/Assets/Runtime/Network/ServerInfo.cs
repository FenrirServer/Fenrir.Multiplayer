using Newtonsoft.Json;

namespace Fenrir.Multiplayer
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
        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        /// <summary>
        /// Unique ID of the server
        /// </summary>
        [JsonProperty("server_id")]
        public string ServerId { get; set; }

        /// <summary>
        /// List of protocols supported by this server
        /// </summary>
        [JsonProperty("protocols")]
        public ProtocolInfo[] Protocols { get; set; } 
    }
}
