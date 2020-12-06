using System;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Network
{
    public interface IProtocolListener : IDisposable
    {
        bool IsRunning { get; }

        ProtocolType ProtocolType { get; }

        IProtocolConnectionData ConnectionData { get; }

        Task Start();

        Task Stop();
    }
}