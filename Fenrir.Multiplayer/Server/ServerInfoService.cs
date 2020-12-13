using Fenrir.Multiplayer.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace Fenrir.Multiplayer.Server
{

    /// <summary>
    /// Server info web service
    /// Returns information about the server
    /// </summary>
    public class ServerInfoService : IServerInfoService
    {
        /// <summary>
        /// Fenrir Server Info Provider
        /// </summary>
        private readonly IFenrirServerInfoProvider _fenrirServerInfoProvider;

        /// <summary>
        /// Instance of the Http Server
        /// </summary>
        private HttpServer _httpServer;

        /// <inheritdoc/>
        public short Port { get; set; } = 8080;

        /// <inheritdoc/>
        public bool IsRunning { get; set; } = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fenrirServerInfoProvider">
        /// Information provider for the Fenrir Server.
        /// Usually, Fenrir Server instance
        /// </param>
        public ServerInfoService(IFenrirServerInfoProvider fenrirServerInfoProvider)
        {
            _fenrirServerInfoProvider = fenrirServerInfoProvider;
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
            if (_fenrirServerInfoProvider.Status != ServerStatus.Running)
            {
                // Send response
                response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                response.Close();
            }

            // Get server info
            var serverInfo = new ServerInfo()
            {
                ServerId = _fenrirServerInfoProvider.ServerId,
                Hostname = _fenrirServerInfoProvider.Hostname,
                Protocols = _fenrirServerInfoProvider.Listeners.Select(
                    listener => new ProtocolInfo()
                    {
                        ProtocolType = listener.ProtocolType,
                        ConnectionData = JObject.FromObject(listener.GetConnectionData())
                    }
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
    }
}
