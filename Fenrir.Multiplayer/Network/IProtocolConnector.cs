
using System;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Network
{
    public interface IProtocolConnector : IDisposable
    {
        IClientPeer Peer { get; }

        Task<ConnectionResult> Connect();

        void Disconnect();
    }
}
