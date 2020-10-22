namespace Fenrir.Multiplayer.Network
{
    public interface IEventHandler<TEvent>
        where TEvent : IEvent
    {
        void OnReceiveEvent(TEvent evt);
    }
}
