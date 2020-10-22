namespace Fenrir.Multiplayer.Network
{
    public enum DisconnectedReason
    {
        ConnectionFailed = 0,
        Timeout = 1,
        HostUnreachable = 2,
        NetworkUnreachable = 3,
        RemoteConnectionClose = 4,
        DisconnectPeerCalled = 5,
        ConnectionRejected = 6,
        InvalidProtocol = 7,
        UnknownHost = 8,
        Reconnect = 9,
        PeerToPeerConnection = 10
    }
}
