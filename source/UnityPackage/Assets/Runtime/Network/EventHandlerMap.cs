using Fenrir.Multiplayer.Exceptions;
using Fenrir.Multiplayer.Logging;
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
        /// Logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Bound handlers
        /// </summary>
        private Dictionary<Type, Action<IEvent>> _eventHandlers = new Dictionary<Type, Action<IEvent>>();

        /// <summary>
        /// Creates event handler map
        /// </summary>
        /// <param name="logger"></param>
        public EventHandlerMap(ILogger logger)
        {
            _logger = logger;
        }

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

            Action<IEvent> handlerAction = evt =>
            {
                try
                {
                    eventHandler.OnReceiveEvent((TEvent)evt);
                }
                catch(Exception e)
                {
                    _logger.Error("Uncaught exception in event {0} handler {1}: {2}", typeof(TEvent).Name, eventHandler, e.ToString());
                }
            };

            lock(_syncRoot)
            {
                if(_eventHandlers.ContainsKey(typeof(TEvent)))
                {
                    throw new EventHandlerException($"Failed to add event handler {eventHandler.GetType()}, handler for event type {typeof(TEvent).Name} is already registered");
                }

                _eventHandlers.Add(typeof(TEvent), handlerAction);
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
                    throw new EventHandlerException($"Failed to remove event handler {eventHandler.GetType()}, handler for event type {typeof(TEvent).Name} is not registered");
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

            bool hasEventHandler = false;
            Action<IEvent> handler = null;

            lock (_syncRoot)
            {
                hasEventHandler = _eventHandlers.TryGetValue(eventType, out handler);
            }

            if(!hasEventHandler)
            {
                _logger.Warning($"Failed to dispatch event of type {eventType}, handler for event type is not registered");
                return;
            }

            handler.Invoke((IEvent)eventWrapper.MessageData);
        }
    }
}
