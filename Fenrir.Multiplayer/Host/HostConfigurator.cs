using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.Exceptions;
using Fenrir.Multiplayer.Network;

namespace Fenrir.Multiplayer.Host
{
    class HostConfigurator : IHostConfigurator
    {
        private ProtocolSet _availableProtocols = new ProtocolSet();

        public void AddProtocol(IProtocol protocol)
        {
            if (_availableProtocols.ContainsKey(protocol.ProtocolType))
            {
                throw new FenrirConfiguratorException($"Failed to add protocol {protocol.ProtocolType}, protocol of that type is already registered");
            }

            _availableProtocols.Add(protocol.ProtocolType, protocol);
        }

        public IFenrirHost BuildHost()
        {
            return new FenrirHost();
        }
    }
}
