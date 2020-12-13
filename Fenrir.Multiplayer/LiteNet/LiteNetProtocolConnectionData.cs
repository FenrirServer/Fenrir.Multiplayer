using Fenrir.Multiplayer.Network;

namespace Fenrir.Multiplayer.LiteNet
{
    /// <summary>
    /// Identifies data require to connect
    /// to LiteNet protocol
    /// </summary>
    class LiteNetProtocolConnectionData : IProtocolConnectionData
    {
        /// <summary>
        /// Public port that client is supposed to use
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// IPv6 Mode
        /// </summary>
        public IPv6ProtocolMode IPv6Mode { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public LiteNetProtocolConnectionData()
        {
        }

        /// <summary>
        /// Creates Lite Net Protocol Connection Data with a given port
        /// </summary>
        /// <param name="port">Public Port</param>
        public LiteNetProtocolConnectionData(ushort port)
            : this()
        {
            Port = port;
            IPv6Mode = IPv6ProtocolMode.Disabled;
        }

        /// <summary>
        /// Creates Lite Net Protocol Connection Data with a given port and ipv6 mode
        /// </summary>
        /// <param name="port">Public Port</param>
        /// <param name="iPv6Mode">IPv6 mode</param>
        public LiteNetProtocolConnectionData(ushort port, IPv6ProtocolMode iPv6Mode)
            : this(port)
        {
            IPv6Mode = iPv6Mode;
        }
    }
}
