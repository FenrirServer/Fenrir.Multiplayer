using Fenrir.Multiplayer.Network;
using System;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Client
{
    class FenrirClient : IFenrirClient, IDisposable
    {
        private readonly IProtocolConnector _connector;

        public IClientPeer Peer => _connector.Peer;

        public FenrirClient(IProtocolConnector connector)
        {
            _connector = connector;
        }

        public Task<ConnectionResult> Connect()
        {
            return _connector.Connect();
        }

        public void Dispose()
        {
            _connector.Dispose();
        }
    }
}
