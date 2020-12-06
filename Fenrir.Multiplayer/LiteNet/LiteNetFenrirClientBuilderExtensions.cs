using Fenrir.Multiplayer.Client;

namespace Fenrir.Multiplayer.LiteNet
{
    public static class LiteNetFenrirClientBuilderExtensions
    {
        public static IFenrirClientBuilder AddLiteNetProtocol(this IFenrirClientBuilder clientBuilder)
        {
            var protocol = new LiteNetProtocol(
                clientBuilder.SerializationProvider, 
                clientBuilder.EventReceiver, 
                clientBuilder.ResponseReceiver, 
                null, // not used by the client 
                clientBuilder.ResponseMap, 
                clientBuilder.TypeMap, 
                clientBuilder.Logger);

            clientBuilder.AddProtocol(protocol);
            return clientBuilder;
        }
    }
}
