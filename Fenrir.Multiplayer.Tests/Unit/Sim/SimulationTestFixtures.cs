using Fenrir.Multiplayer.Sim;
using Fenrir.Multiplayer.Sim.Command;
using Fenrir.Multiplayer.Sim.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fenrir.Multiplayer.Tests.Unit.Sim
{
    class TestSimulationListener : ISimulationListener
    {
        public List<ISimulationCommand> Commands { get; private set; } = new List<ISimulationCommand>();

        void ISimulationListener.OnSendCommand(ISimulationCommand command)
        {
            Commands.Add(command);
        }

        void ISimulationListener.OnSendCommands(IEnumerable<ISimulationCommand> commands)
        {
            Commands.AddRange(commands);
        }

        public bool HasCommand<T>() where T : ISimulationCommand
        {
            return Commands.Any(cmd => cmd is T);
        }

        public bool TryGetCommand<T>(out T command) where T : ISimulationCommand
        {
            command = GetCommand<T>();
            return command != null;
        }

        public T GetCommand<T>() where T : ISimulationCommand
        {
            return (T)Commands.Where(cmd => cmd is T).FirstOrDefault();
        }
    }


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
