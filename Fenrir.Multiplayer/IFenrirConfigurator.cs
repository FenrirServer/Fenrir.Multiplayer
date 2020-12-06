using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.Server;

namespace Fenrir.Multiplayer
{
    public interface IFenrirConfigurator
    {
        IClientConfigurator ConfigureClient();

        IHostConfigurator ConfigureHost();
    }
}