using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;

namespace Fenrir.Multiplayer.LiteNet
{
    class LiteNetProtocolConnectorFactory : IProtocolConnectorFactory
    {
        private string _hostname;
        private short _port;

        public LiteNetProtocolConnectorFactory(string hostname, short port)
        {
            _hostname = hostname;
            _port = port;
        }

        public IProtocolConnector Create(
            string clientId,
            object connectionData,
            ISerializationProvider serializerProvider, 
            IEventReceiver eventReceiver, 
            IResponseReceiver responseReceiver, 
            IResponseMap responseMap, 
            ITypeMap typeMap, 
            IPv6ProtocolMode ipv6Mode)
        {
            return new LiteNetProtocolConnector(_hostname, _port, clientId, connectionData, serializerProvider, eventReceiver, responseReceiver, responseMap, typeMap, ipv6Mode);
        }
    }
}
