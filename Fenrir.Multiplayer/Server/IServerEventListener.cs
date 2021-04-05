using Fenrir.Multiplayer.LiteNet;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using System.Net;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Server
{
    interface IServerEventListener
    {
        Task<ConnectionResponse> HandleConnectionRequest(int protocolVersion, string clientId, IPEndPoint endPoint, IByteStreamReader connectionDataReader);
        
        void OnReceiveRequest(IServerPeer serverPeer, MessageWrapper messageWrapper);

        void OnReceiveRawMessage(IServerPeer serverPeer, ushort messageCode, IByteStreamReader byteStreamReader);

        void OnPeerConnected(IServerPeer serverPeer);

        void OnPeerDisconnected(IServerPeer serverPeer);
    }
}
