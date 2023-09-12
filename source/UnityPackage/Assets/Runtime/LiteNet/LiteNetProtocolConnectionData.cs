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
        public bool IPv6Enabled { get; set; }

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
            IPv6Enabled = false;
        }

        /// <summary>
        /// Creates Lite Net Protocol Connection Data with a given port and ipv6 mode
        /// </summary>
        /// <param name="port">Public Port</param>
        /// <param name="iPv6Enabled">Enable or Disable IPv6 Support</param>
        public LiteNetProtocolConnectionData(ushort port, bool iPv6Enabled)
            : this(port)
        {
            IPv6Enabled = iPv6Enabled;
        }
    }
}
