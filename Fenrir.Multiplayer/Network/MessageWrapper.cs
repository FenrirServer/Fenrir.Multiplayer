namespace Fenrir.Multiplayer.Network
{
    public struct MessageWrapper
    {
        public MessageType MessageType;
        public int RequestId;
        public object MessageData;
        public IPeerInternal Peer;
        public byte Channel;
        public MessageDeliveryMethod DeliveryMethod;
    }
}
