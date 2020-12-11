namespace Fenrir.Multiplayer.Network
{
    public interface IServerPeer : IPeer
    {
        int Latency { get; }

        void SendEvent<TEvent>(TEvent evt, byte channel = 0, MessageDeliveryMethod deliveryMethod = MessageDeliveryMethod.ReliableOrdered)
            where TEvent : IEvent;
    }
}
