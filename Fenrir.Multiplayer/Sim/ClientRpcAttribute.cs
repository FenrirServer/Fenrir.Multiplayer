using System;

namespace Fenrir.Multiplayer.Sim
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ClientRpcAttribute : Attribute
    {
    }
}