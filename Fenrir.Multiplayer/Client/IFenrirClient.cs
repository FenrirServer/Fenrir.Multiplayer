
using Fenrir.Multiplayer.Network;
using System;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Client
{
    /// <summary>
    /// Fenrir Client
    /// Connects to a FenrirServer using given protocols
    /// </summary>
    public interface IFenrirClient
    {
        /// <summary>
        /// Unique id of the client
        /// </summary>
        string ClientId { get; set; }

        /// <summary>
        /// Client Peer object. null if client is not connected
        /// </summary>
        IClientPeer Peer { get; }

        /// <summary>
        /// Adds supported protocol. 
        /// At least one protocol should be added in order to connect to a server.
        /// </summary>
        /// <param name="protocol">Protocol to add</param>
        void AddProtocol(IProtocol protocol);

        /// <summary>
        /// Connects using Server Info URI
        /// Server Info URI will be queried to obtain server data
        /// </summary>
        /// <param name="serverInfoUri">Server Info URI to query</param>
        /// <param name="connectionRequestData">Custom connection request data</param>
        /// <returns>Connection Result</returns>
        Task<ClientConnectionResult> Connect(Uri serverInfoUri, object connectionRequestData = null);

        /// <summary>
        /// Connects using Server Info object
        /// Directly connects to a server with existing Server Info object
        /// </summary>
        /// <param name="serverInfo">Server Info object - contains information about server</param>
        /// <param name="connectionRequestData">Custom connection request data</param>
        /// <returns>Connection Result</returns>
        Task<ClientConnectionResult> Connect(ServerInfo serverInfo, object connectionRequestData = null);
    }
}
