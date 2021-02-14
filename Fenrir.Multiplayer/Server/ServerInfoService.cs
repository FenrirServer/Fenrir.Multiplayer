using Fenrir.Multiplayer.Network;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace Fenrir.Multiplayer.Server
{

    /// <summary>
    /// Server info web service
    /// Server Info Service is a simple web (http) service that returns
    /// server status as well as connection information
    /// </summary>
    class ServerInfoService : IServerInfoService
    {
        /// <summary>
        /// Fenrir NetworkServer Info Provider
        /// </summary>
        private readonly IServerInfoProvider _networkServerInfoProvider;

        /// <summary>
        /// Instance of the Http Server
        /// </summary>
        private HttpServer _httpServer;

        /// <inheritdoc/>
        public ushort Port { get; set; } = 8080;

        /// <inheritdoc/>
        public bool IsRunning => _httpServer?.IsListening ?? false;

        /// <summary>
        /// Creates Server Info Service
        /// </summary>
        /// <param name="networkServerInfoProvider">
        /// Information provider for the Fenrir NetworkServer.
        /// Normally, a Network Server instance 
        /// </param>
        public ServerInfoService(IServerInfoProvider networkServerInfoProvider)
        {
            _networkServerInfoProvider = networkServerInfoProvider;
        }

        /// <summary>
        /// Creates Server Info Service
        /// </summary>
        /// <param name="networkServerInfoProvider">
        /// Information provider for the Fenrir Server.
        /// Normally, a Network Server instance 
        /// </param>
        public ServerInfoService(IServerInfoProvider networkServerInfoProvider, ushort port)
            : this(networkServerInfoProvider)
        {
            Port = port;
        }


        /// <inheritdoc/>
        public Task Start()
        {
            if (!IsRunning)
            {
                _httpServer = new HttpServer(Port);
                _httpServer.OnGet += OnHttpServerGet;
                _httpServer.Start();
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task Stop()
        {
            if (IsRunning)
            {
                _httpServer.Stop();
                _httpServer = null;
            }

            return Task.CompletedTask;
        }

        private void OnHttpServerGet(object sender, HttpRequestEventArgs e)
        {
            var response = e.Response;

            // Check server status
            if (_networkServerInfoProvider.Status != ServerStatus.Running)
            {
                // Send response
                response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                response.Close();
            }

            // Get server info
            var serverInfo = new ServerInfo()
            {
                ServerId = _networkServerInfoProvider.ServerId,
                Hostname = _networkServerInfoProvider.Hostname,
                Protocols = _networkServerInfoProvider.Listeners.Select(
                    listener => new ProtocolInfo(listener.ProtocolType, listener.GetConnectionData())
                ).ToArray()
            };

            // Create response
            string contentsString = JsonConvert.SerializeObject(serverInfo);
            byte[] contentsByte = Encoding.UTF8.GetBytes(contentsString);

            // Send response
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "application/json";
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = contentsByte.LongLength;
            response.Close(contentsByte, true);
        }

        public void Dispose()
        {
            Stop().Wait();
        }
    }
}
