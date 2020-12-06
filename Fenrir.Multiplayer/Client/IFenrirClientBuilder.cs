using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using System;
using System.Net.Http;

namespace Fenrir.Multiplayer.Client
{
    /// <summary>
    /// Helper interface to build the Fenrir Client
    /// </summary>
    public interface IFenrirClientBuilder
    {
        /// <summary>
        /// Serialization Provider
        /// </summary>
        ISerializationProvider SerializationProvider { get; }

        /// <summary>
        /// Event Receiver
        /// </summary>
        IEventReceiver EventReceiver { get; }

        /// <summary>
        /// Response Receiver
        /// </summary>
        IResponseReceiver ResponseReceiver { get; }

        /// <summary>
        /// Response Map
        /// </summary>
        IResponseMap ResponseMap { get; }

        /// <summary>
        /// Type Map
        /// </summary>
        ITypeMap TypeMap { get; }

        /// <summary>
        /// Logger
        /// </summary>
        IFenrirLogger Logger { get; }


        /// <summary>
        /// Http Client
        /// </summary>
        HttpClient HttpClient { get; }


        /// <summary>
        /// Specifies Serialization Provider to use
        /// </summary>
        /// <param name="serializationProvider">Serialization Provider</param>
        /// <returns>Current instance of the Fenrir Client Builder</returns>
        IFenrirClientBuilder UseSerializationProvider(ISerializationProvider serializationProvider);

        /// <summary>
        /// Specifies Event Receiver to use
        /// </summary>
        /// <param name="eventReceiver">Event Receiver</param>
        /// <returns>Current instance of the Fenrir Client Builder</returns>
        IFenrirClientBuilder UseEventReceiver(IEventReceiver eventReceiver);

        /// <summary>
        /// Specifies Response Receiver to use
        /// </summary>
        /// <returns>Current instance of the Fenrir Client Builder</returns>
        /// <returns></returns>
        IFenrirClientBuilder UseResponseReceiver(IResponseReceiver responseReceiver);

        /// <summary>
        /// Specifies Response Map to use
        /// </summary>
        /// <param name="responseMap">Response Map</param>
        /// <returns>Current instance of the Fenrir Client Builder</returns>
        IFenrirClientBuilder UseResponseMap(IResponseMap responseMap);

        /// <summary>
        /// Specifies Type Map to use
        /// </summary>
        /// <param name="typeMap">Type Map</param>
        /// <returns>Current instance of the Fenrir Client Builder</returns>
        IFenrirClientBuilder UseTypeMap(ITypeMap typeMap);

        /// <summary>
        /// Specifies Logger to use
        /// </summary>
        /// <param name="logger">Fenrir Logger</param>
        /// <returns>Current instance of the Fenrir Client Builder</returns>
        IFenrirClientBuilder UseLogger(IFenrirLogger logger);

        /// <summary>
        /// Specifies HttpClient to use
        /// </summary>
        /// <param name="httpClient">Http Client</param>
        /// <returns>Current instance of the Fenrir Client Builder</returns>
        IFenrirClientBuilder UseHttpClient(HttpClient httpClient);

        /// <summary>
        /// Adds supported protocol
        /// </summary>
        /// <param name="protocol">Protocol</param>
        /// <returns>Current instance of the Fenrir Client Builder</returns>
        IFenrirClientBuilder AddProtocol(IProtocol protocol);

        /// <summary>
        /// Adds configuration handler that will be executed upon Build
        /// </summary>
        /// <param name="handler">Configuration handler</param>
        /// <returns>Current instance of the Fenrir Client Builder</returns>
        IFenrirClientBuilder Configure(Action<IFenrirClient> handler);

        /// <summary>
        /// Builds the Fenrir Client
        /// </summary>
        /// <returns>Current instance of the Fenrir Client Builder</returns>
        IFenrirClient Build();
    }
}