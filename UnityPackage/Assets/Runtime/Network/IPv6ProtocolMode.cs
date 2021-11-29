namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Identifies IPv6 support
    /// </summary>
    public enum IPv6ProtocolMode
    {
        /// <summary>
        /// No IPv6 is supported
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// IPv6 connections are accepted via separate socket
        /// </summary>
        SeparateSocket = 1,

        /// <summary>
        /// Single socket is used to accept both IPv4 and IPv6 connections
        /// </summary>
        DualMode = 2
    }
}
