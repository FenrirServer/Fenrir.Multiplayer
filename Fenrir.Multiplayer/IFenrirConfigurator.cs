using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.Host;

namespace Fenrir.Multiplayer
{
    public interface IFenrirConfigurator
    {
        IClientConfigurator ConfigureClient();

        IHostConfigurator ConfigureHost();
    }
}