using Fenrir.Multiplayer.Serialization;

namespace Fenrir.Multiplayer.Network
{
    public interface IProtocolConnectorFactory
    {
        IProtocolConnector Create(
            string clientId, 
            object connectionData, 
            ISerializationProvider serializerProvider, 
            IEventReceiver eventReceiver, 
            IResponseReceiver responseReceiver, 
            IResponseMap responseMap, 
            ITypeMap typeMap, 
            IPv6ProtocolMode ipv6Mode
        );
    }
}
