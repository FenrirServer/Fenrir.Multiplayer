﻿using System.Net;

namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Server connection request
    /// </summary>
    public interface IServerConnectionRequest
    {
        /// <summary>
        /// Remote endpoint
        /// </summary>
        IPEndPoint Endpoint { get;  }

        /// <summary>
        /// Client Id of the incoming connection
        /// </summary>
        string ClientId { get; }

        /// <summary>
        /// Version of the protocol used by this client
        /// </summary>
        int ProtocolVersion { get; }
    }

    /// <summary>
    /// Server connection request
    /// </summary>
    /// <typeparam name="TConnectionRequestData">Custom data parameter</typeparam>

    public interface IServerConnectionRequest<TConnectionRequestData> : IServerConnectionRequest
    {
        /// <summary>
        /// Connection request data
        /// </summary>
        TConnectionRequestData Data { get; }
    }
}