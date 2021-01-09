using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Sim.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;


namespace Fenrir.Multiplayer.Sim
{
    public class SimulationObject
    {
        /// <summary>
        /// Logger
        /// </summary>
        protected IFenrirLogger Logger { get; private set; }

        /// <summary>
        /// Reference to a simulation object
        /// </summary>
        public Simulation Simulation { get; private set; }

        /// <summary>
        /// Unique id of the object
        /// </summary>
        public ushort Id { get; private set; }

        /// <summary>
        /// List of components, by component type
        /// </summary>
        private OrderedDictionary _componentsByType = new OrderedDictionary();

        /// <summary>
        /// Tick when object was created
        /// </summary>
        public int TickCreated { get; private set; }

        /// <summary>
        /// Tick when simulation object has been destroyed
        /// </summary>
        public int TickDestroyed { get; private set; }

        /// <summary>
        /// Indicates if object has been destroyed
        /// </summary>
        public bool IsDestroyed { get; private set; }


        public SimulationObject(Simulation simulation, IFenrirLogger logger, ushort objectId)
        {
            Simulation = simulation;
            Logger = logger;
            TickCreated = simulation.CurrentTick;
            Id = objectId;
        }

        public void AddComponent<TComponent>(TComponent component)
             where TComponent : SimulationComponent
        {
            if(!Simulation.IsServer)
            {
                throw new SimulationException("Client simulation is not allowed to add components directly. Please invoke server RPC using a component");
            }

            if(component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (_componentsByType.Contains(typeof(TComponent)))
            {
                throw new ArgumentException($"Failed to add component {typeof(TComponent).Name}, object already has component of this type");
            }

            if(!Simulation.ComponentRegistered<TComponent>())
            {
                throw new ArgumentException($"Failed to add component {typeof(TComponent).Name}, component type is not registered with Simulation. Please call {nameof(Simulation.RegisterComponentType)}");
            }

            ulong componentTypeHash = Simulation.GetComponentTypeHash<TComponent>();

            _componentsByType.Add(typeof(TComponent), component);

            component.OnAdded(this);
        }

        public TComponent AddComponent<TComponent>()
            where TComponent : SimulationComponent, new()
        {
            if (!Simulation.IsServer)
            {
                throw new SimulationException("Client simulation is not allowed to add components directly. Please invoke server RPC using a component");
            }

            if (!Simulation.ComponentRegistered<TComponent>())
            {
                throw new ArgumentException($"Failed to add component {typeof(TComponent).Name}, component type is not registered with Simulation. Please call {nameof(Simulation.RegisterComponentType)}");
            }

            TComponent component = new TComponent();
            AddComponent(component);
            return component;
        }

        public TComponent GetComponent<TComponent>() 
            where TComponent : SimulationComponent
        {
            if(_componentsByType.Contains(typeof(TComponent)))
            {
                return (TComponent)_componentsByType[typeof(TComponent)];
            }

            return null;
        }

        public bool TryGetComponent<TComponent>(out TComponent component) 
            where TComponent : SimulationComponent
        {
            component = GetComponent<TComponent>();
            return component != null;
        }

        public IEnumerable<SimulationComponent> GetComponents()
        {
            IDictionaryEnumerator componentEnumerator = _componentsByType.GetEnumerator();

            while (componentEnumerator.MoveNext())
            {
                SimulationComponent component = (SimulationComponent)componentEnumerator.Value;
                yield return component;
            }
        }

        public TComponent GetOrAddComponent<TComponent>()
            where TComponent : SimulationComponent, new()
        {
            if (!Simulation.IsServer)
            {
                throw new SimulationException("Client simulation is not allowed to add components directly. Please invoke server RPC using a component");
            }

            if (TryGetComponent<TComponent>(out TComponent component))
            {
                return component;
            }

            return AddComponent<TComponent>();
        }

        public void RemoveComponent<TComponent>()
             where TComponent : SimulationComponent
        {
            if (!Simulation.IsServer)
            {
                throw new SimulationException("Client simulation is not allowed to remove components directly. Please invoke server RPC using a component");
            }

            if (!_componentsByType.Contains(typeof(TComponent)))
            {
                throw new ArgumentException($"Failed to remove component {typeof(TComponent).Name}, not in component collection");
            }

            TComponent component = (TComponent)_componentsByType[typeof(TComponent)];
            _componentsByType.Remove(typeof(TComponent));

            component.OnRemoved();
        }

        public virtual void Tick()
        {
            // Get all components attached to this object and tick them
            foreach (var component in GetComponents())
            {
                try
                {
                    component.Tick();
                }
                catch (Exception e)
                {
                    Logger.Error($"Uncaught exception during component {nameof(SimulationComponent.Tick)}: {e.ToString()}");
                }
            }
        }

        public void Destroy()
        {
            IsDestroyed = true;
            TickDestroyed = Simulation.CurrentTick;
        }
    }
}
