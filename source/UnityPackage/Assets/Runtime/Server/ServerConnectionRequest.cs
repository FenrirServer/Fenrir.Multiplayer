using System.Net;

namespace Fenrir.Multiplayer.Server
{
    /// <summary>
    /// Server Connection Request
    /// </summary>
    /// <typeparam name="TConnectionRequestData">Custom data</typeparam>
    class ServerConnectionRequest<TConnectionRequestData> : IServerConnectionRequest<TConnectionRequestData>
    {
        /// <inheritdoc/>
        public IPEndPoint Endpoint { get; private set; }

        /// <inheritdoc/>
        public int ProtocolVersion { get; private set; }

        /// <inheritdoc/>
        public string ClientId { get; private set; }

        /// <inheritdoc/>

        public TConnectionRequestData Data { get; private set; }

        /// <summary>
        /// Creates new server connection request
        /// </summary>
        /// <param name="endpoint">Remote endpoint</param>
        /// <param name="protocolVersion">Version of the protocol used by this client</param>
        /// <param name="clientId">Client id</param>
        /// <param name="connectionRequestData">Custom Connection Request data object</param>
        public ServerConnectionRequest(IPEndPoint endpoint, int protocolVersion, string clientId, TConnectionRequestData connectionRequestData)
        {
            Endpoint = endpoint;
            ProtocolVersion = protocolVersion;
            ClientId = clientId;
            Data = connectionRequestData;
        }
    }
}
