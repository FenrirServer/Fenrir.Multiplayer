namespace Fenrir.Multiplayer.Simulation.Command
{
    public enum CommandType : byte
    {
        SpawnObject,

        DestroyObject,

        AddComponent,

        RemoveComponent,

        ServerRpc,

        ClientRpc,

    }
}
