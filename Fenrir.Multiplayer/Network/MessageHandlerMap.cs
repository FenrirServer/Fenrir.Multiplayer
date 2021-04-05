using Fenrir.Multiplayer.Exceptions;
using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Raw message map
    /// Similar to <seealso cref="RequestHandlerMap"/> or <seealso cref="EventHandlerMap"/>
    /// Stores raw message handlers bound to unique message codes
    /// </summary>
    class MessageHandlerMap
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
        private Dictionary<ushort, Func<IByteStreamReader, IPeer, Task>> _messageHandlers = new Dictionary<ushort, Func<IByteStreamReader, IPeer, Task>>();

        /// <summary>
        /// Creates event handler map
        /// </summary>
        /// <param name="logger"></param>
        public MessageHandlerMap(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Adds raw message handler for a given message code
        /// </summary>
        /// <param name="messageCode">Message code</param>
        /// <param name="messageHandler">Message handler</param>
        public void AddMessageHandler(ushort messageCode, IRawMessageHandlerAsync messageHandler)
        {
            if(messageHandler == null)
            {
                throw new ArgumentNullException(nameof(messageHandler));
            }

            Func<IByteStreamReader, IPeer, Task> handlerAction = (reader, peer) =>
            {
                try
                {
                    return messageHandler.OnReceiveMessageAsync(messageCode, reader, peer);
                }
                catch (Exception e)
                {
                    _logger.Error("Uncaught exception in raw message handler {0} for the message code {1}: {2}", messageHandler, messageCode, e.ToString());
                    return Task.CompletedTask;
                }
            };

            lock (_syncRoot)
            {
                if (_messageHandlers.ContainsKey(messageCode))
                {
                    throw new MessageHandlerException($"Failed to add message handler {messageHandler.GetType()}, handler for message code {messageCode} is already registered");
                }

                _messageHandlers.Add(messageCode, handlerAction);
            }
        }

        /// <summary>
        /// Removes message handler for a given message code
        /// </summary>
        /// <param name="messageCode">Message code</param>
        public void RemoveMessageHandler(ushort messageCode)
        {
            lock (_syncRoot)
            {
                if (!_messageHandlers.ContainsKey(messageCode))
                {
                    throw new MessageHandlerException($"Failed to remove message handler, handler for message code {messageCode} is not registered");
                }

                _messageHandlers.Remove(messageCode);
            }
        }

        /// <summary>
        /// Invoked when new raw message is received
        /// </summary>
        /// <param name="messageCode">Message Code</param>
        /// <param name="reader">Byte Stream Raeder</param>
        /// <param name="peer">Peer that sent the message</param>
        /// <returns>Task that completes when done reading message and provided byte stream can be released to the object pool</returns>
        public Task OnReceiveRawMessage(ushort messageCode, IByteStreamReader reader, IPeer peer)
        {
            bool hasMessageHandler = false;
            Func<IByteStreamReader, IPeer, Task> handler = null;

            lock (_syncRoot)
            {
                hasMessageHandler = _messageHandlers.TryGetValue(messageCode, out handler);
            }

            if (!hasMessageHandler)
            {
                _logger.Warning($"Failed to dispatch raw message with code {messageCode}, handler for message code is not registered");
                return Task.CompletedTask;
            }

            return handler.Invoke(reader, peer);
        }
    }
}
