using Fenrir.Multiplayer.Logging;
using System.Collections;
using System.Collections.Specialized;

namespace Fenrir.Multiplayer.Sim
{
    public class SimulationServerObject : SimulationObject
    {
        /// <summary>
        /// Temporary buffer of removed components by component type. 
        /// Removed components are stored in this buffer for the next number of ticks equal to <see cref="ServerSimulation.SnapshotHistoryBufferSizeTicks"/> 
        /// </summary>
        private OrderedDictionary _removedComponentsByType = new OrderedDictionary();


        public SimulationServerObject(Simulation simulation, IFenrirLogger logger, ushort objectId)
            : base(simulation, logger, objectId)
        {
        }

        public override void Tick()
        {
            base.Tick();

            // Expire removed components            
            IDictionaryEnumerator componentEnumerator = _removedComponentsByType.GetEnumerator();

            while (componentEnumerator.MoveNext())
            {
                SimulationComponent component = (SimulationComponent)componentEnumerator.Value;
                ServerSimulation serverSim = (ServerSimulation)Simulation;
                if (component.TickRemoved > Simulation.CurrentTick + serverSim.SnapshotHistoryBufferSizeTicks)
                {
                    _removedComponentsByType.Remove(component.GetType());
                }
            }
        }
    }
}
