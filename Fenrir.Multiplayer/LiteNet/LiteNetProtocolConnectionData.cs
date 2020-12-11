using Fenrir.Multiplayer.Network;

namespace Fenrir.Multiplayer.LiteNet
{
    class LiteNetProtocolConnectionData : IProtocolConnectionData
    {
        public string Hostname => HostnameV4;

        public string HostnameV4 { get; set; }

        public string HostnameV6 { get; set; }

        public ushort Port { get; set; }

        public IPv6ProtocolMode IPv6Mode { get; set; }

        public LiteNetProtocolConnectionData(string hostnameV4, string hostnameV6, ushort port, IPv6ProtocolMode iPv6Mode)
        {
            HostnameV4 = hostnameV4;
            HostnameV6 = hostnameV6;
            Port = port;
            IPv6Mode = iPv6Mode;
        }
    }
}
