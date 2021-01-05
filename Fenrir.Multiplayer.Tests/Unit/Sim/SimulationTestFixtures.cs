using Fenrir.Multiplayer.Sim;
using Fenrir.Multiplayer.Sim.Components;
using System;

namespace Fenrir.Multiplayer.Tests.Unit.Sim
{
    class TestComponent : SimulationComponent
    {
        public string Value;

        public TestComponent()
        {
            Value = "test";
        }

        public TestComponent(string value)
        {
            Value = value;
        }
    }

    class OtherTestComponent : SimulationComponent
    {
    }

    class TestPlayerComponent : PlayerComponent
    {
        public string PlayerName;

        public TestPlayerComponent(string peerId, string playerName) : base(peerId)
        {
            PlayerName = playerName;
        }
    }

    class TestTickingComponent : SimulationComponent
    {
        public Action TickHandler;

        public override void Tick()
        {
            TickHandler?.Invoke();
        }
    }
}
