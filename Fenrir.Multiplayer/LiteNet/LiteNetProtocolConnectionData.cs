using Fenrir.Multiplayer.Network;

namespace Fenrir.Multiplayer.LiteNet
{
    /// <summary>
    /// Identifies data require to connect
    /// to LiteNet protocol
    /// </summary>
    class LiteNetProtocolConnectionData : IProtocolConnectionData
    {
        public ushort Port { get; set; }

        public IPv6ProtocolMode IPv6Mode { get; set; }

        public LiteNetProtocolConnectionData(ushort port, IPv6ProtocolMode iPv6Mode)
        {
            Port = port;
            IPv6Mode = iPv6Mode;
        }
    }
}
