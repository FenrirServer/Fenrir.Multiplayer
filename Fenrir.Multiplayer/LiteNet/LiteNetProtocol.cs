using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using System;

namespace Fenrir.Multiplayer.LiteNet
{
    class LiteNetProtocol : IProtocol
    {
        public ProtocolType ProtocolType => ProtocolType.LiteNet;

        public Type ConnectionDataType => typeof(LiteNetProtocolConnectionData);

        private readonly ISerializationProvider _serializationProvider;
        private readonly IEventReceiver _eventReceiver;
        private readonly IResponseReceiver _responseReceiver;
        private readonly IRequestReceiver _requestReceiver;
        private readonly IResponseMap _responseMap;
        private readonly ITypeMap _typeMap;
        private readonly IFenrirLogger _logger;

        public LiteNetProtocol(ISerializationProvider serializationProvider, 
            IEventReceiver eventReceiver, 
            IResponseReceiver responseReceiver, 
            IRequestReceiver requestReceiver, 
            IResponseMap responseMap, 
            ITypeMap typeMap,
            IFenrirLogger logger)
        {
            _serializationProvider = serializationProvider;
            _eventReceiver = eventReceiver;
            _responseReceiver = responseReceiver;
            _requestReceiver = requestReceiver;
            _responseMap = responseMap;
            _typeMap = typeMap;
            _logger = logger;
        }

        public IProtocolConnector CreateConnector()
        {
            return new LiteNetProtocolConnector(_serializationProvider, _eventReceiver, _responseReceiver, _responseMap, _logger, _typeMap);
        }

        public IProtocolListener CreateListener()
        {
            return new LiteNetProtocolListener(_serializationProvider, _requestReceiver, _logger, _typeMap);
        }
    }
}
