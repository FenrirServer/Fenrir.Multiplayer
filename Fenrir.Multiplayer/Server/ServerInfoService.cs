using Fenrir.Multiplayer.Network;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Server
{

    /// <summary>
    /// Server info web service
    /// Returns information about the server
    /// </summary>
    public class ServerInfoService : IServerInfoService
    {
        private const string ServerInfoEndpoint = "/";
        
        private readonly IFenrirServer _fenrirServer;

        private IWebHost _webHost;
        
        public string HostName { get; set; } = "0.0.0.0";

        public short Port { get; set; } = 8080;

        private string BaseUri => string.Format("http://{0}:{1}", HostName, Port);

        public ServerInfoService(IFenrirServer fenrirServer)
        {
            _fenrirServer = fenrirServer;
        }

        public async Task Start()
        {
            _webHost = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(new string[] { BaseUri })
                .ConfigureServices(builderContext => builderContext.AddRouting())
                .Configure(appBuilder =>
                {
                    var routeBuilder = new RouteBuilder(appBuilder);
                    routeBuilder.MapGet(ServerInfoEndpoint, GetServerInfo);
                    appBuilder.UseRouter(routeBuilder.Build());
                })
                .Build();

            await _webHost.StartAsync();
        }

        private async Task GetServerInfo(HttpContext httpContext)
        {
            var serverInfo = new ServerInfo()
            {
                ServerId = _fenrirServer.ServerId,
                Protocols = _fenrirServer.Listeners.Select(
                    listener => new ProtocolInfo() {
                        ProtocolType = listener.ProtocolType,
                        ConnectionData = JObject.FromObject(listener.ConnectionData)
                    }    
                ).ToArray()
            };

            await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(serverInfo));
        }
    }
}
