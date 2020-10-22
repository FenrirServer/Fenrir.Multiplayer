using Fenrir.Multiplayer.Network;

namespace Fenrir.Multiplayer.LiteNet
{
    class LiteNetProtocol : IProtocol
    {
        public ProtocolType ProtocolType { get; private set; }

        public IProtocolConnectorFactory ConnectorFactory { get; private set; }

        public LiteNetProtocol(string hostname, short port)
        {
            ProtocolType = ProtocolType.LiteNet;
            ConnectorFactory = new LiteNetProtocolConnectorFactory(hostname, port);
        }
    }
}
