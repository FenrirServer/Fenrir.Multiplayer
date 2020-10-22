using Fenrir.Multiplayer.Serialization;

namespace Fenrir.Multiplayer.Network
{
    public interface IProtocolListenerFactory
    {
        IProtocolListener Create(
            ISerializationProvider serializerProvider,
            IRequestReceiver requestReceiver,
            ITypeMap typeMap,
            IPv6ProtocolMode ipv6Mode
        );
    }
}
