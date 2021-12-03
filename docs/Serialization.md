# Connection Request

This section explains the basics of network serialization and transferring data over the wire.

Previous Section: [Reliability](/Reliability.md)

## Byte Stream Serialization

By default, Fenrir Multiplayer comes with a byte stream serialization mechanism for messages and data objects.
It is simple but powerful: it allows explicitly serializing messages into directly into a byte buffer that is used to write into a socket.

In order to use byte stream serializables, your messages should implement `IByteStreamSerializable`:

```csharp
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Network;

class MyEvent : IEvent, IByteStreamSerializable
{
    // Field that we want to write into a buffer
    public string Name;

    // Invoked during serialization
    public void Serialize(IByteStreamWriter writer)
    {
        // Write string into a byte buffer
        writer.Write(Name);
    }

    // Invoked during deserialization
    public void Deserialize(IByteStreamReader reader)
    {
        // Read string from the byte buffer
        Name = reader.ReadString();
    }
}
```

You can write simple types such as strings, integers, enums etc. 
You can also write nested types that implement IByteStreamSerializable:

```csharp
class Player : IByteStreamSerializable
{
    public string Name;
    public int Health;

    public void Serialize(IByteStreamWriter writer)
    {
        writer.Write(Name);
        writer.Write(Health);
    }

    public void Deserialize(IByteStreamReader reader)
    {
        Name = reader.ReadString();
        Health = reader.ReadInt();
    }
}

class MyEvent : IEvent, IByteStreamSerializable
{
    public Player Name;

    public void Serialize(IByteStreamWriter writer)
    {
        writer.Write(Player);
    }

    public void Deserialize(IByteStreamReader reader)
    {
        Player = reader.Read<Player>();
    }
}
```

Arrays, dictionaries and lists are also supported:

```csharp
class RoundStartEvent : IEvent, IByteStreamSerializable
{
    public Player[] Players;
    public Dictionary<string, Player> PlayersByName;
    public List<Player> PlayerList;

    public void Serialize(IByteStreamWriter writer)
    {
        writer.Write(Players);
        writer.Write(PlayersByName);
        writer.Write(PlayerList);
    }

    public void Deserialize(IByteStreamReader reader)
    {
        Players = reader.Read<Player[]>();
        PlayersByName = reader.Read<Dictionary<string, Player>>();
        PlayerList = reader.Read<List<Player>>();
    }
}
```

Next Section: [Custom Type Serialization](/CustomTypeSerialization.md)
