namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Event handler of a specific event type
    /// Invoked when event of a given type is received
    /// </summary>
    /// <typeparam name="TEvent">Type of event</typeparam>
    public interface IEventHandler<TEvent>
        where TEvent : IEvent
    {
        /// <summary>
        /// Invoked when event of the given type is received
        /// </summary>
        /// <param name="evt">Event object</param>
        void OnReceiveEvent(TEvent evt);
    }
}
