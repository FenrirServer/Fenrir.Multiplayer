using Fenrir.Multiplayer.Logging;
using System;

namespace Fenrir.Multiplayer.Simulation
{
    public abstract partial class SimulationComponent
    {
        public SimulationObject Object { get; private set; }

        public NetworkSimulation Simulation => Object?.Simulation;

        public uint InitializedTickNumber { get; private set; }

        public uint DestroyedTickNumber { get; private set; }

        protected ILogger Logger => Object?.Logger;

        internal ulong TypeHash { get; private set; }

        internal ComponentTypeWrapper TypeWrapper { get; private set; }


        internal void Initialize(SimulationObject simulationObject)
        {
            Type componentType = GetType();

            Object = simulationObject;
            InitializedTickNumber = simulationObject.Simulation.CurrentTickNumber;
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
            DestroyedTickNumber = Simulation.CurrentTickNumber;
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
