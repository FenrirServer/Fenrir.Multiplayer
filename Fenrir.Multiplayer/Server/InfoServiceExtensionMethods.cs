namespace Fenrir.Multiplayer.Server
{
    /// <summary>
    /// ServerInfoService extension methods
    /// </summary>
    public static class InfoServiceExtensionMethods
    {
        /// <summary>
        /// Enables Server Info Service for NetworkServer.
        /// Server Info Service is a simple HTTP server that reports status
        /// of the server as well as nessesary connection data
        /// </summary>
        /// <param name="server">Server</param>
        public static void AddInfoService(this NetworkServer server)
        {
            server.AddService(new ServerInfoService(server));
        }

        /// <summary>
        /// Enables Server Info Service for NetworkServer.
        /// Server Info Service is a simple HTTP server that reports status
        /// of the server as well as nessesary connection data
        /// </summary>
        /// <param name="server">Server</param>
        /// <param name="port">Port</param>
        public static void AddInfoService(this NetworkServer server, ushort port)
        {
            server.AddService(new ServerInfoService(server, port));
        }
    }
}
