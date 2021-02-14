using Fenrir.Multiplayer.Client;

namespace Fenrir.Multiplayer.LiteNet
{
    /// <summary>
    /// LiteNet Extension methods for Fenrir Network Client
    /// </summary>
    public static class NetworkClientExtensionMethods
    {
        /// <summary>
        /// Adds LiteNet Protocol support
        /// </summary>
        /// <param name="client">Client</param>
        public static void AddLiteNetProtocol(this NetworkClient client)
        {
            client.AddProtocol(new LiteNetProtocolConnector());
        }
    }
}
