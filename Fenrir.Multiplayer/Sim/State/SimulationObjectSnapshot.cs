using System.Collections.Specialized;

namespace Fenrir.Multiplayer.Sim.State
{
    struct SimulationObjectSnapshot
    {
        public ushort ObjectId { get; private set; }

        public OrderedDictionary ComponentsByType { get; private set; }
    }
}
