namespace Fenrir.Multiplayer.LiteNet
{
    public static class LiteNetConfiguratorExtensions
    {
        public static IFenrirConfigurator AddLiteNet(this IFenrirConfigurator configurator, string hostname, short port)
        {
            var protocol = new LiteNetProtocol(hostname, port);

            configurator.ConfigureClient().AddProtocol(protocol);
            configurator.ConfigureHost().AddProtocol(protocol);

            return configurator;
        }
    }
}
