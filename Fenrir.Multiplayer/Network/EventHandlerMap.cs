using Fenrir.Multiplayer.Exceptions;
using System;
using System.Collections.Generic;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Map that stores bound event handles
    /// Event handlers are bound to the specific event type
    /// </summary>
    class EventHandlerMap
    {
        /// <summary>
        /// Sync root
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Bound handlers
        /// </summary>
        private Dictionary<Type, Action<IEvent>> _eventHandlers = new Dictionary<Type, Action<IEvent>>();

        /// <summary>
        /// Binds event handler to an event type
        /// </summary>
        /// <typeparam name="TEvent">Event Type</typeparam>
        /// <param name="eventHandler">Event Handler</param>
        public void AddEventHandler<TEvent>(IEventHandler<TEvent> eventHandler)
            where TEvent : IEvent
        {
            if(typeof(TEvent) == typeof(IEvent))
            {
                throw new InvalidOperationException("Attempting to add event handler for IEvent. Please specify generic type <TEvent> explicitly.");
            }

            lock(_syncRoot)
            {
                if(_eventHandlers.ContainsKey(typeof(TEvent)))
                {
                    throw new EventListenerException($"Failed to add event handler {eventHandler.GetType()}, handler for event type {typeof(TEvent).Name} is already registered");
                }

                _eventHandlers.Add(typeof(TEvent), evt => eventHandler.OnReceiveEvent((TEvent)evt));
            }
        }

        /// <summary>
        /// Removes event handler of a specific event type
        /// </summary>
        /// <typeparam name="TEvent">Event Type</typeparam>
        /// <param name="eventHandler">Event Handler</param>
        public void RemoveEventHandler<TEvent>(IEventHandler<TEvent> eventHandler)
            where TEvent : IEvent
        {
            if (typeof(TEvent) == typeof(IEvent))
            {
                throw new InvalidOperationException("Attempting to remove event handler for IEvent. Please specify generic type <TEvent> explicitly.");
            }

            lock (_syncRoot)
            {
                if (_eventHandlers.ContainsKey(typeof(TEvent)))
                {
                    throw new EventListenerException($"Failed to remove event handler {eventHandler.GetType()}, handler for event type {typeof(TEvent).Name} is not registered");
                }

                _eventHandlers.Remove(typeof(TEvent));
            }
        }

        /// <summary>
        /// Invoked when event is received
        /// </summary>
        /// <param name="eventWrapper">Message Wrapper</param>
        public void OnReceiveEvent(MessageWrapper eventWrapper)
        {
            Type eventType = eventWrapper.MessageData.GetType();

            Action<IEvent> handler = null;

            lock (_syncRoot)
            {
                if (!_eventHandlers.ContainsKey(eventType))
                {
                    throw new EventListenerException($"Failed to dispatch event of type {eventType}, handler for event type is not registered");
                }

                handler = _eventHandlers[eventType];
            }

            handler.Invoke((IEvent)eventWrapper.MessageData);
        }
    }
}
