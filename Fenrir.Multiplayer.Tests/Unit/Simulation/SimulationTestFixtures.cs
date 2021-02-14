using Fenrir.Multiplayer.Simulation;
using Fenrir.Multiplayer.Simulation.Command;
using Fenrir.Multiplayer.Simulation.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fenrir.Multiplayer.Tests.Unit.Simulation
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

    class TestClientRpcComponent : SimulationComponent
    {
        public bool BoolParam;
        public float FloatParam;
        public string StringParam;
        public int IntParam;

        public TestClientRpcComponent()
        {
        }

        [ClientRpc]
        public void DoTest(bool boolParam, float floatParam, string stringParam, int intParam)
        {
            BoolParam = boolParam;
            FloatParam = floatParam;
            StringParam = stringParam;
            IntParam = intParam;
        }
    }

    class TestServerRpcComponent : SimulationComponent
    {
        public bool BoolParam;
        public float FloatParam;
        public string StringParam;
        public int IntParam;

        public TestServerRpcComponent()
        {
        }

        [ServerRpc]
        public void DoTest(bool boolParam, float floatParam, string stringParam, int intParam)
        {
            BoolParam = boolParam;
            FloatParam = floatParam;
            StringParam = stringParam;
            IntParam = intParam;
        }
    }
}
