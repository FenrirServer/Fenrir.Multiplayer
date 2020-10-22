using Fenrir.Multiplayer.Network;

namespace Fenrir.Multiplayer.Host
{
    public interface IHostConfigurator
    {
        void AddProtocol(IProtocol protocol);
    }
}