namespace Fenrir.Multiplayer.Network
{
    public interface IProtocol
    {
        ProtocolType ProtocolType { get; }
        IProtocolConnectorFactory ConnectorFactory { get; }
    }
}
