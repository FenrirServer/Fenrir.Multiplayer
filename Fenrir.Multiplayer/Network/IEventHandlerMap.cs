namespace Fenrir.Multiplayer.Network
{
    public interface IEventHandlerMap
    {
        void AddEventHandler<TEvent>(IEventHandler<TEvent> eventHandler)
            where TEvent : IEvent;

        void RemoveEventHandler<TEvent>(IEventHandler<TEvent> eventHandler)
            where TEvent : IEvent;
    }
}