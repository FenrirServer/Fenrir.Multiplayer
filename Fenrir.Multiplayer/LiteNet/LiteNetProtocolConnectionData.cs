using Fenrir.Multiplayer.Network;

namespace Fenrir.Multiplayer.LiteNet
{
    public class LiteNetProtocolConnectionData : IProtocolConnectionData
    {
        public string Hostname { get; set; }

        public ushort Port { get; set; }

        public IPv6ProtocolMode IPv6Mode { get; set; }
    }
}
