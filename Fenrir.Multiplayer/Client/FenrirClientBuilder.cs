using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Fenrir.Multiplayer.Client
{
    /// <summary>
    /// Fenrir Client Builder
    /// Helper class that allows to build and configure instance of the Fenrir Client
    /// </summary>
    public class FenrirClientBuilder : IFenrirClientBuilder
    {
        /// <summary>
        /// List of action handlers executed upon Build
        /// </summary>
        private List<Action<IFenrirClient>> _builderActions = new List<Action<IFenrirClient>>();

        /// <inheritdoc/>
        public ISerializationProvider SerializationProvider { get; private set; }

        /// <inheritdoc/>
        public IEventReceiver EventReceiver { get; private set; }

        /// <inheritdoc/>
        public IResponseReceiver ResponseReceiver { get; private set; }

        /// <inheritdoc/>
        public IResponseMap ResponseMap { get; private set; }

        /// <inheritdoc/>
        public ITypeMap TypeMap { get; private set; }

        /// <inheritdoc/>
        public IFenrirLogger Logger { get; private set; }

        /// <inheritdoc/>
        public HttpClient HttpClient { get; private set; }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public FenrirClientBuilder()
        {
            // Dependency defaults - override with Use* methods
            SerializationProvider = new SerializationProvider();
            EventReceiver = new EventHandlerMap();
            TypeMap = new TypeMap();
            Logger = new EventBasedLogger();

            var requestResponseMap = new RequestResponseMap();
            ResponseReceiver = requestResponseMap;
            ResponseMap = requestResponseMap;
        }

        /// <inheritdoc/>
        public IFenrirClientBuilder AddProtocol(IProtocol protocol)
        {
            Configure(client => client.AddProtocol(protocol));
            return this;
        }

        /// <inheritdoc/>
        public IFenrirClientBuilder Configure(Action<IFenrirClient> handler)
        {
            _builderActions.Add(handler);
            return this;
        }

        /// <inheritdoc/>
        public IFenrirClientBuilder UseSerializationProvider(ISerializationProvider serializationProvider)
        {
            SerializationProvider = serializationProvider;
            return this;
        }

        /// <inheritdoc/>
        public IFenrirClientBuilder UseEventReceiver(IEventReceiver eventReceiver)
        {
            EventReceiver = eventReceiver;
            return this;
        }

        /// <inheritdoc/>
        public IFenrirClientBuilder UseResponseReceiver(IResponseReceiver responseReceiver)
        {
            ResponseReceiver = responseReceiver;
            return this;
        }

        /// <inheritdoc/>
        public IFenrirClientBuilder UseResponseMap(IResponseMap responseMap)
        {
            ResponseMap = responseMap;
            return this;
        }

        /// <inheritdoc/>
        public IFenrirClientBuilder UseTypeMap(ITypeMap typeMap)
        {
            TypeMap = typeMap;
            return this;
        }

        /// <inheritdoc/>
        public IFenrirClientBuilder UseLogger(IFenrirLogger logger)
        {
            Logger = logger;
            return this;
        }

        /// <inheritdoc/>
        public IFenrirClientBuilder UseHttpClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
            return this;
        }


        /// <inheritdoc/>
        public IFenrirClient Build()
        {
            var client = new FenrirClient(HttpClient);

            foreach (var action in _builderActions)
            {
                action(client);
            }

            return client;
        }

    }
}
