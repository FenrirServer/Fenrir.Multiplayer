using Fenrir.Multiplayer.Sim.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;


namespace Fenrir.Multiplayer.Sim
{
    public sealed class SimulationObject
    {
        public Simulation Simulation { get; private set; }

        public ushort Id { get; private set; }

        private OrderedDictionary _componentsByType = new OrderedDictionary();


        public SimulationObject(Simulation simulation, ushort objectId)
        {
            Simulation = simulation;
            Id = objectId;
        }


        public void AddComponent<TComponent>(TComponent component)
             where TComponent : SimulationComponent
        {
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
            if(TryGetComponent<TComponent>(out TComponent component))
            {
                return component;
            }

            return AddComponent<TComponent>();
        }

        public void RemoveComponent<TComponent>()
             where TComponent : SimulationComponent
        {
            if (!_componentsByType.Contains(typeof(TComponent)))
            {
                throw new ArgumentException($"Failed to remove component {typeof(TComponent).Name}, not in component collection");
            }

            TComponent component = (TComponent)_componentsByType[typeof(TComponent)];
            _componentsByType.Remove(typeof(TComponent));

            component.OnRemoved();
        }
    }
}
