namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// State of the client connection
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// Disconnected from server
        /// </summary>
        Disconnected,

        /// <summary>
        /// Connecting to server
        /// </summary>
        Connecting,

        /// <summary>
        /// Connected to server
        /// </summary>
        Connected,
    }
}
