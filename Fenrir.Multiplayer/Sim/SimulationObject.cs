using Fenrir.Multiplayer.Logging;
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
        /// Temporary buffer of removed components by component type. 
        /// Removed components are stored in this buffer until max rollback time 
        /// </summary>
        private OrderedDictionary _removedComponentsByType = new OrderedDictionary();

        /// <summary>
        /// Time when object was created
        /// </summary>
        public DateTime TimeCreated { get; private set; }

        /// <summary>
        /// Time when simulation object has been destroyed
        /// </summary>
        public DateTime TimeDestroyed { get; private set; }

        /// <summary>
        /// Indicates if object has been destroyed
        /// </summary>
        public bool IsDestroyed { get; private set; }


        public SimulationObject(Simulation simulation, IFenrirLogger logger, ushort objectId)
        {
            Simulation = simulation;
            Logger = logger;
            TimeCreated = DateTime.UtcNow;
            Id = objectId;
        }

        #region AddComponent
        public TComponent AddComponent<TComponent>()
            where TComponent : SimulationComponent
        {
            // This method is a simple facade around internal Simulation method that creates a component addition command
            // Command is executed and replicated on all clients
            return Simulation.AddComponent<TComponent>(this);
        }

        internal void AddComponent<TComponent>(TComponent component)
            where TComponent : SimulationComponent
        {
            if (_componentsByType.Contains(typeof(TComponent)))
            {
                throw new ArgumentException($"Failed to add component {typeof(TComponent).Name}, object already has component of this type");
            }

            // Add to list of components
            _componentsByType.Add(typeof(TComponent), component);
        }
        #endregion

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
            if (TryGetComponent<TComponent>(out TComponent component))
            {
                return component;
            }

            return AddComponent<TComponent>();
        }

        public void RemoveComponent<TComponent>()
             where TComponent : SimulationComponent
        {
            // This method is a simple facade around internal Simulation method that replicates component removal command to all clients,
            // then calls RemoveComponent(Type componentType)
            Simulation.AddComponent<TComponent>(this);
        }

        internal void RemoveComponent(Type componentType)
        {
            if (!_componentsByType.Contains(componentType))
            {
                throw new ArgumentException($"Failed to remove component {componentType.Name}, component not added to the object component list");
            }

            // Get component
            SimulationComponent component = (SimulationComponent)_componentsByType[componentType];
            
            // Remove from the list
            _componentsByType.Remove(componentType);

            // Re-add to the list of removed components, allowing to get it's state if we roll back this operation
            _removedComponentsByType.Add(componentType, component);

            // Invoked component callback
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

            // Expire any removed components            
            IDictionaryEnumerator componentEnumerator = _removedComponentsByType.GetEnumerator();

            while (componentEnumerator.MoveNext())
            {
                SimulationComponent component = (SimulationComponent)componentEnumerator.Value;
                if (component.TimeRemoved > Simulation.Time + TimeSpan.FromMilliseconds(Simulation.MaxRollbackTimeMs))
                {
                    // We can't rollback past this point anymore, so ready to remove this component (and let GC destroy it)
                    _removedComponentsByType.Remove(component.GetType());
                }
            }
        }

        public void Destroy()
        {
            IsDestroyed = true;
            TimeDestroyed = DateTime.UtcNow;
        }
    }
}
