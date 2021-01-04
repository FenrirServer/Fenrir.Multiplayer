namespace Fenrir.Multiplayer.Sim.Components
{
    public abstract class PlayerComponent : SimulationComponent
    {
        public string PeerId { get; set; }

        public PlayerComponent(string peerId)
        {
            PeerId = peerId;
        }
    }
}
