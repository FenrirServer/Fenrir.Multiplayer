﻿using System.Net;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer
{
    interface IServerEventListener
    {
        Task<ConnectionHandlerResult> HandleConnectionRequest(int protocolVersion, string clientId, IPEndPoint endPoint, IByteStreamReader connectionDataReader);
        
        void OnReceiveRequest(IServerPeer serverPeer, MessageWrapper messageWrapper);

        void OnPeerConnected(IServerPeer serverPeer);

        void OnPeerDisconnected(IServerPeer serverPeer);
    }
}
