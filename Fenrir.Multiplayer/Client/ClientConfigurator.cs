using Fenrir.Multiplayer.Exceptions;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using System;
using System.Linq;

namespace Fenrir.Multiplayer.Client
{
    class ClientConfigurator : IClientConfigurator
    {
        private readonly ProtocolSet _availableProtocols = new ProtocolSet();
        private readonly SerializationProvider _serializerProvider = new SerializationProvider();

        private string _clientId;
        private object _connectionData;
        private IEventReceiver _eventReceiver;
        private IResponseReceiver _responseReceiver;
        private IResponseMap _responseMap;
        private ITypeMap _typeMap;
        private IPv6ProtocolMode _ipv6Mode;

        public ClientConfigurator()
        {
        }

        public IClientConfigurator ConfigureDefault()
        {
            _clientId = Guid.NewGuid().ToString();
            _connectionData = null;
            _eventReceiver = new EventHandlerMap();

            var responseMap  = new RequestResponseMap();
            _responseReceiver = responseMap;
            _responseMap = responseMap;

            _typeMap = new TypeMap();
            _ipv6Mode = IPv6ProtocolMode.Disabled;

            return this;
        }

        public IClientConfigurator UseClientId(string clientId)
        {
            _clientId = clientId;
            return this;
        }

        public IClientConfigurator UseConnectionData(object connectionData)
        {
            _connectionData = connectionData;
            return this;
        }

        public IClientConfigurator UseSerializer(IContractSerializer serializer)
        {
            _serializerProvider.ContractSerializer = serializer;
            return this;
        }

        public IClientConfigurator UseIPv6(IPv6ProtocolMode mode)
        {
            _ipv6Mode = mode;
            return this;
        }

        public IClientConfigurator RegisterNetworkType(Type type)
        {
            _typeMap.AddType(type);
            return this;
        }

        public IClientConfigurator AddProtocol(IProtocol protocol)
        {
            if(_availableProtocols.ContainsKey(protocol.ProtocolType))
            {
                throw new FenrirConfiguratorException($"Failed to add protocol {protocol.ProtocolType}, protocol of that type is already registered");
            }

            _availableProtocols.Add(protocol.ProtocolType, protocol);

            return this;
        }

        public IClientConfigurator AddProtocol(ProtocolType protocolType, IProtocol protocol)
        {
            _availableProtocols.Add(protocolType, protocol);
            return this;
        }

        public IFenrirClient BuildClient()
        {
            if(_availableProtocols.Count == 0)
            {
                throw new FenrirConfiguratorException("Failed to build Fenrir Client: no available protocols configured.");
            }

            var protocol = _availableProtocols.Values.First();

            return BuildClient(protocol);
        }

        public IFenrirClient BuildClient(ProtocolType protocolType)
        {
            if (!_availableProtocols.ContainsKey(protocolType))
            {
                throw new FenrirConfiguratorException($"Failed to build Fenrir Client: protocol {protocolType} is not configured.");
            }

            return BuildClient(_availableProtocols[protocolType]);
        }

        private IFenrirClient BuildClient(IProtocol protocol)
        {
            var protocolConnector = protocol.ConnectorFactory.Create(
                _clientId,
                _connectionData, 
                _serializerProvider,
                _eventReceiver, 
                _responseReceiver, 
                _responseMap, 
                _typeMap, 
                _ipv6Mode
            );

            return new FenrirClient(protocolConnector);
        }
    }
}
