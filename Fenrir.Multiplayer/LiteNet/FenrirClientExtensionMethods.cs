using Fenrir.Multiplayer.Client;

namespace Fenrir.Multiplayer.LiteNet
{
    /// <summary>
    /// LiteNet Extension methods for Fenrir Client
    /// </summary>
    public static class FenrirClientExtensionMethods
    {
        /// <summary>
        /// Adds LiteNet Protocol support
        /// </summary>
        /// <param name="client">Client</param>
        public static void AddLiteNetProtocol(this FenrirClient client)
        {
            client.AddProtocol(new LiteNetProtocolConnector());
        }
    }
}
