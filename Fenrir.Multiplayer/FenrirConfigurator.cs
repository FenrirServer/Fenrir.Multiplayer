using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.Host;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fenrir.Multiplayer
{
    public class FenrirConfigurator : IFenrirConfigurator
    {
        private ClientConfigurator _clientConfigurator;
        private HostConfigurator _hostConfigurator;

        public static IFenrirConfigurator CreateDefault()
        {
            return new FenrirConfigurator();
        }

        public FenrirConfigurator()
        {
            _clientConfigurator = new ClientConfigurator();
            _hostConfigurator = new HostConfigurator();
        }

        public IClientConfigurator ConfigureClient()
        {
            return _clientConfigurator;
        }

        public IHostConfigurator ConfigureHost()
        {

            return _hostConfigurator;
        }

        public IFenrirClient BuildClient()
        {
            return _clientConfigurator.BuildClient();
        }

        public IFenrirHost BuildHost()
        {
            return _hostConfigurator.BuildHost();
        }

    }
}
