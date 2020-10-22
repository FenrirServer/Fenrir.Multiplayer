using System.Net;

namespace Fenrir.Multiplayer.Host
{
    public class HostConnectionRequest<TConnectionData>
    {
        public IPEndPoint Endpoint { get; private set; }

        public string ClientId { get; private set; }

        public TConnectionData ConnectionData { get; private set; }

        internal HostConnectionRequest(IPEndPoint endpoint, string clientId, TConnectionData connectionData)
        {
            Endpoint = endpoint;
            ClientId = clientId;
            ConnectionData = connectionData;
        }
    }
}
