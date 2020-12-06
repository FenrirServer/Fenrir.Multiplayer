using System;

namespace Fenrir.Multiplayer.Network
{
    public interface IProtocol
    {
        ProtocolType ProtocolType { get; }

        Type ConnectionDataType { get; }

        IProtocolConnector CreateConnector();

        IProtocolListener CreateListener();
    }
}
