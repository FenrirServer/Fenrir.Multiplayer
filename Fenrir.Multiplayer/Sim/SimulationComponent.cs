using Fenrir.Multiplayer.Logging;
using System;

namespace Fenrir.Multiplayer.Sim
{
    public abstract class SimulationComponent
    {
        public SimulationObject Object { get; private set; }

        public Simulation Simulation => Object?.Simulation;

        public ulong TypeHash { get; private set; }

        public DateTime TimeInitialized { get; private set; }

        public DateTime TimeDestroyed { get; private set; }

        protected IFenrirLogger Logger => Object?.Logger;

        internal void Initialize(SimulationObject simulationObject)
        {
            TimeInitialized = DateTime.UtcNow;
            Object = simulationObject;
            TypeHash = Object.Simulation.GetComponentTypeHash(GetType());

            // Invoke callback
            OnAdded();
        }
        internal void BeforeDestroy()
        {
            OnBeforeDestroyed();
        }

        internal void Destroy()
        {
            TimeDestroyed = DateTime.UtcNow;
            Object = null;
            OnDestroyed();
        }

        internal void Tick()
        {
            OnTick();
        }

        internal void LateTick()
        {
            OnLateTick();
        }

        protected virtual void OnAdded() { }

        protected virtual void OnBeforeDestroyed() { }

        protected virtual void OnDestroyed() { }

        protected virtual void OnTick(){ }

        protected virtual void OnLateTick() { }
    }
}
