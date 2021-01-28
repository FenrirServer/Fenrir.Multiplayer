using Fenrir.Multiplayer.Sim;
using Fenrir.Multiplayer.Sim.Command;
using Fenrir.Multiplayer.Sim.Components;
using System;
using System.Collections.Generic;
using System.Linq;

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


    class TestTickingComponent : SimulationComponent
    {
        public Action TickHandler;

        protected override void OnTick()
        {
            TickHandler?.Invoke();
        }
    }
}
