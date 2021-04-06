namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Represents a protocol server listener
    /// </summary>
    public interface IProtocolListener
    {
        /// <summary>
        /// Indicates if protocol is listening
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Type of the protocol
        /// </summary>
        ProtocolType ProtocolType { get; }

        /// <summary>
        /// Returns protocol connection data, required to pass by the client when connecting using this protocol
        /// </summary>
        IProtocolConnectionData GetConnectionData();
    }
}