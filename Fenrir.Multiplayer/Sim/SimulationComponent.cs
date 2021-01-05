namespace Fenrir.Multiplayer.Sim
{
    public abstract class SimulationComponent
    {
        public SimulationObject Object { get; private set; }

        public virtual void OnAdded(SimulationObject simulationObject)
        {
            Object = simulationObject;
        }

        public virtual void OnRemoved()
        {
            Object = null;
        }

        public virtual void Tick()
        {
        }
    }
}
