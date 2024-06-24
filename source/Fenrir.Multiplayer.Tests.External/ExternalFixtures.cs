namespace Fenrir.Multiplayer.Tests
{
    // Type with the same structure, Name and Namespace as one in the Fenrir.Multiplayer.Tests assembly
    // This file is included in both assemblies: Fenrir.Multiplayer.Tests and Fenrir.Multiplayer.Tests.External
    public class TestExternalClass : IByteStreamSerializable
    {
        public string Value;

        public void Deserialize(IByteStreamReader reader)
        {
            Value = reader.ReadString();
        }

        public void Serialize(IByteStreamWriter writer)
        {
            writer.Write(Value);
        }

        public TestExternalClass() 
        { 
        }
    }
}
