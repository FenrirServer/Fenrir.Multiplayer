using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Server;

namespace Fenrir.Multiplayer.LiteNet
{
    /// <summary>
    /// LiteNet Extension methods for Fenrir Server
    /// </summary>
    public static class FenrirServerExtensionMethods
    {
        /// <summary>
        /// Adds LiteNet Protocol support
        /// </summary>
        /// <param name="server">Server</param>
        /// <param name="port">Port</param>
        public static void AddLiteNetProtocol(this FenrirServer server)
        {
            server.AddProtocol(new LiteNetProtocolListener());
        }

        /// <summary>
        /// Adds LiteNet Protocol support
        /// </summary>
        /// <param name="server">Server</param>
        /// <param name="port">Port</param>
        public static void AddLiteNetProtocol(this FenrirServer server, ushort port)
        {
            server.AddProtocol(new LiteNetProtocolListener(port));
        }

        /// <summary>
        /// Adds LiteNet Protocol support
        /// </summary>
        /// <param name="server">Server</param>
        /// <param name="port">Port</param>
        /// <param name="bindIPv4">IPv4 Listen Address</param>
        public static void AddLiteNetProtocol(this FenrirServer server, ushort port, string bindIPv4)
        {
            server.AddProtocol(new LiteNetProtocolListener(port, bindIPv4));
        }

        /// <summary>
        /// Adds LiteNet Protocol support
        /// </summary>
        /// <param name="server">Server</param>
        /// <param name="port">Port</param>
        /// <param name="bindIPv4">IPv4 Listen Address</param>
        /// <param name="bindIPv6">IPv6 Listen Address</param>
        /// <param name="ipv6Mode">IPv6 Mode</param>
        public static void AddLiteNetProtocol(this FenrirServer server, ushort port, string bindIPv4, string bindIPv6, IPv6ProtocolMode ipv6Mode)
        {
            server.AddProtocol(new LiteNetProtocolListener(port, bindIPv4, bindIPv6, ipv6Mode));
        }
    }
}
