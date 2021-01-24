using System;

namespace Fenrir.Multiplayer.Sim
{
    public abstract class SimulationComponent
    {
        public SimulationObject Object { get; private set; }

        public DateTime TimeAdded { get; private set; }

        public DateTime TimeRemoved { get; private set; }

        public virtual void OnAdded(SimulationObject simulationObject)
        {
            TimeAdded = DateTime.UtcNow;
            Object = simulationObject;
        }

        public virtual void OnRemoved()
        {
            TimeRemoved = DateTime.UtcNow;
            Object = null;
        }

        public virtual void Tick()
        {
        }
    }
}
