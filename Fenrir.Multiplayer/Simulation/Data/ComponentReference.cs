using Fenrir.Multiplayer.Serialization;

namespace Fenrir.Multiplayer.Simulation.Data
{
    /// <summary>
    /// Simple structure used to serialize component instance reference over the network
    /// </summary>
    struct ComponentReference
    {
        public ushort ObjectId { get; private set; }

        public ulong ComponentTypeHash { get; private set; }

        public ComponentReference(ushort objectId, ulong componentTypeHash)
        {
            ObjectId = objectId;
            ComponentTypeHash = componentTypeHash;
        }

        public ComponentReference(SimulationComponent component)
        {
            ObjectId = component.Object.Id;
            ComponentTypeHash = component.TypeHash;
        }
    }
}
