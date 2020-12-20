using System.Net;

namespace Fenrir.Multiplayer.Server
{
    /// <summary>
    /// Represents incoming server connection
    /// </summary>
    /// <typeparam name="TConnectionRequestData">Custom request data object</typeparam>
    public class ServerConnectionRequest<TConnectionRequestData>
    {
        /// <summary>
        /// IP Endpoint of the incoming connection
        /// </summary>
        public IPEndPoint Endpoint { get; private set; }

        /// <summary>
        /// Client Id of the incoming connection
        /// </summary>
        public string ClientId { get; private set; }

        /// <summary>
        /// Custom connection request data object
        /// </summary>
        public TConnectionRequestData Data { get; private set; }

        internal ServerConnectionRequest(IPEndPoint endpoint, string clientId, TConnectionRequestData connectionRequestData)
        {
            Endpoint = endpoint;
            ClientId = clientId;
            Data = connectionRequestData;
        }
    }
}
