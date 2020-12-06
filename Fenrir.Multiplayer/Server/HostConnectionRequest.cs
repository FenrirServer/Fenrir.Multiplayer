using System.Net;

namespace Fenrir.Multiplayer.Server
{
    public class HostConnectionRequest<TConnectionRequestData>
    {
        public IPEndPoint Endpoint { get; private set; }

        public string ClientId { get; private set; }

        public TConnectionRequestData ConnectionRequestData { get; private set; }

        internal HostConnectionRequest(IPEndPoint endpoint, string clientId, TConnectionRequestData connectionRequestData)
        {
            Endpoint = endpoint;
            ClientId = clientId;
            ConnectionRequestData = connectionRequestData;
        }
    }
}
