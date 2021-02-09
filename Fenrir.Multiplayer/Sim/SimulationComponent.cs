using Fenrir.Multiplayer.Logging;
using System;

namespace Fenrir.Multiplayer.Sim
{
    public abstract partial class SimulationComponent
    {
        public SimulationObject Object { get; private set; }

        public Simulation Simulation => Object?.Simulation;

        public DateTime TimeInitialized { get; private set; }

        public DateTime TimeDestroyed { get; private set; }

        protected IFenrirLogger Logger => Object?.Logger;

        internal ulong TypeHash { get; private set; }

        internal ComponentTypeWrapper TypeWrapper { get; private set; }


        internal void Initialize(SimulationObject simulationObject)
        {
            Type componentType = GetType();

            TimeInitialized = DateTime.UtcNow;
            Object = simulationObject;
            TypeHash = Object.Simulation.GetComponentTypeHash(componentType);
            TypeWrapper = Object.Simulation.GetComponentWrapper(componentType);

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
