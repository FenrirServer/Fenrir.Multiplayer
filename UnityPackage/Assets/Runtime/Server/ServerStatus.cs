namespace Fenrir.Multiplayer.Server
{
    /// <summary>
    /// Represents status of the server
    /// </summary>
    public enum ServerStatus
    {
        /// <summary>
        /// Server is stopped
        /// </summary>
        Stopped,

        /// <summary>
        /// Server is starting
        /// </summary>
        Starting,

        /// <summary>
        /// Server is running
        /// </summary>
        Running,

        /// <summary>
        /// Server is shutting down
        /// </summary>
        Stopping,
    }
}
