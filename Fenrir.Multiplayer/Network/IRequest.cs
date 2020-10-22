using System;
using System.Collections.Generic;
using System.Text;

namespace Fenrir.Multiplayer.Network
{
    public interface IRequest
    {
    }

    public interface IRequest<TResponse> : IRequest
        where TResponse : IResponse
    {
    }
}
