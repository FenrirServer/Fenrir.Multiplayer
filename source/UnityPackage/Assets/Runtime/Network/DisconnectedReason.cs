namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Reason for a disconnect
    /// </summary>
    public enum DisconnectedReason
    {
        /// <summary>
        /// Failed to connect to server
        /// </summary>
        ConnectionFailed = 0,

        /// <summary>
        /// Connection timed out
        /// </summary>
        Timeout = 1,

        /// <summary>
        /// Could not reach destination host
        /// </summary>
        HostUnreachable = 2,

        /// <summary>
        /// Network is not availble
        /// </summary>
        NetworkUnreachable = 3,

        /// <summary>
        /// Remote connection was closed
        /// </summary>
        RemoteConnectionClose = 4,

        /// <summary>
        /// Peer was disconnected by server
        /// </summary>
        DisconnectPeerCalled = 5,

        /// <summary>
        /// Connection was rejected by server
        /// </summary>
        ConnectionRejected = 6,

        /// <summary>
        /// Unknown protocol
        /// </summary>
        InvalidProtocol = 7,

        /// <summary>
        /// Host is unknown
        /// </summary>
        UnknownHost = 8,

        /// <summary>
        /// Peer must reconnect
        /// </summary>
        Reconnect = 9,

        /// <summary>
        /// Lost Peer to Peer connection
        /// </summary>
        PeerToPeerConnection = 10
    }
}
