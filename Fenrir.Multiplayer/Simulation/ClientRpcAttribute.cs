using System;

namespace Fenrir.Multiplayer.Simulation
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ClientRpcAttribute : Attribute
    {
    }
}