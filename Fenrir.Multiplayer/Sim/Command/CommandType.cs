namespace Fenrir.Multiplayer.Sim.Command
{
    public enum CommandType : byte
    {
        SpawnObject,

        DestroyObject,

        AddComponent,

        RemoveComponent,

        ServerRpc,
    }
}
