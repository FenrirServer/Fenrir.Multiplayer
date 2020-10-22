using Fenrir.Multiplayer.Network;

namespace Fenrir.Multiplayer.Client
{
    public interface IClientConfigurator
    {
        IClientConfigurator ConfigureDefault();

        IClientConfigurator AddProtocol(IProtocol protocol);

        IFenrirClient BuildClient();

        IFenrirClient BuildClient(ProtocolType protocolType);
    }
}