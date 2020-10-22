namespace Fenrir.Multiplayer.Network
{
    interface IHostPeer : IPeer
    {
        void SendEvent<TEvent>(TEvent evt, byte channel = 0, MessageDeliveryMethod deliveryMethod = MessageDeliveryMethod.ReliableOrdered)
            where TEvent : IEvent;
    }
}
