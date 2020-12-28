using Fenrir.Multiplayer.Exceptions;
using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Server.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Server
{
    /// <summary>
    /// Fenrir Server Host
    /// </summary>
    public class FenrirServer : IFenrirServer
    {
        /// <inheritdoc/>
        public event EventHandler<ServerStatusChangedEventArgs> StatusChanged;

        /// <inheritdoc/>
        public event EventHandler<ProtocolAddedEventArgs> ProtocolAdded;

        /// <inheritdoc/>
        public string ServerId { get; set; }

        /// <inheritdoc/>
        public string Hostname { get; set; } = "127.0.0.1";

        /// <inheritdoc/>
        public IEnumerable<IProtocolListener> Listeners => _protocolListeners;

        /// <inheritdoc/>
        public ServerStatus Status => _status;

        /// <inheritdoc/>
        public bool IsRunning => Status == ServerStatus.Running;

        /// <summary>
        /// List of available protocols
        /// </summary>
        private List<IProtocolListener> _protocolListeners;

        /// <summary>
        /// List of services that this server is using
        /// </summary>
        private List<IFenrirService> _services;

        /// <summary>
        /// Fenrir Logger
        /// </summary>
        private IFenrirLogger _logger;

        /// <summary>
        /// Server status
        /// </summary>
        private volatile ServerStatus _status = ServerStatus.Stopped;

        /// <summary>
        /// Creates Fenrir Server
        /// </summary>
        public FenrirServer()
            : this(new EventBasedLogger())
        {
        }

        /// <summary>
        /// Creates Fenrir Server
        /// </summary>
        /// <param name="logger">Logger</param>
        public FenrirServer(IFenrirLogger logger)
        {
            ServerId = Guid.NewGuid().ToString();

            _logger = logger;

            _protocolListeners = new List<IProtocolListener>();
            _services = new List<IFenrirService>();
        }

        /// <inheritdoc/>
        public async Task Start()
        {
            if(Status != ServerStatus.Stopped)
            {
                throw new InvalidOperationException("Failed to start server, server is already " + Status);
            }

            SetStatus(ServerStatus.Starting);

            // Start all services
            await Task.WhenAll(_services.Select(service => service.Start()));

            // Start all protocol listeners
            await Task.WhenAll(_protocolListeners.Select(listener => listener.Start()));

            // Check that all listeners has started properly and were not stopped during the start
            foreach(var protocolListener in _protocolListeners)
            {
                if(!protocolListener.IsRunning)
                {
                    throw new FenrirServerException($"Failed to start server, protocol listener {protocolListener.ProtocolType} is not running");
                }
            }

            SetStatus(ServerStatus.Running);
        }

        /// <inheritdoc/>
        public async Task Stop()
        {
            if (Status != ServerStatus.Running && Status != ServerStatus.Starting)
            {
                throw new InvalidOperationException("Failed to stop server, server is already " + Status);
            }

            SetStatus(ServerStatus.Stopping);

            // Stop all protocol listeners
            await Task.WhenAll(_protocolListeners.Select(listener => listener.Stop()));

            // Stop all services
            await Task.WhenAll(_services.Select(service => service.Stop()));

            SetStatus(ServerStatus.Stopped);
        }


        /// <inheritdoc/>
        public void SetLogger(IFenrirLogger logger)
        {
            foreach (var protocolListener in _protocolListeners)
            {
                protocolListener.SetLogger(logger);
            }

            ProtocolAdded += (sender, e) => {
                e.ProtocolListener.SetLogger(logger);
            };
        }

        /// <inheritdoc/>
        public void SetContractSerializer(IContractSerializer contractSerializer)
        {
            foreach (var protocolListener in _protocolListeners)
            {
                protocolListener.SetContractSerializer(contractSerializer);
            }

            ProtocolAdded += (sender, e) => {
                e.ProtocolListener.SetContractSerializer(contractSerializer);
            };
        }

        /// <inheritdoc/>
        public void AddProtocol(IProtocolListener protocolListener)
        {
            if(protocolListener == null)
            {
                throw new ArgumentNullException(nameof(protocolListener));
            }

            _protocolListeners.Add(protocolListener);

            ProtocolAdded?.Invoke(this, new ProtocolAddedEventArgs(protocolListener));
        }

        /// <inheritdoc/>
        public void AddService(IFenrirService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            _services.Add(service);
        }

        /// <summary>
        /// Sets server status and invokes the event
        /// </summary>
        private void SetStatus(ServerStatus status)
        {
            _status = status;

            StatusChanged?.Invoke(this, new ServerStatusChangedEventArgs(status));
        }

        /// <inheritdoc/>
        public void SetConnectionRequestHandler<TConnectionRequestData>(Func<IServerConnectionRequest<TConnectionRequestData>, Task<ConnectionResponse>> handler) 
            where TConnectionRequestData : class, new()
        {
            if(handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            foreach(var protocolListener in _protocolListeners)
            {
                protocolListener.SetConnectionRequestHandler<TConnectionRequestData>(handler);
            }

            ProtocolAdded += (sender, e) => {
                e.ProtocolListener.SetConnectionRequestHandler<TConnectionRequestData>(handler);
            };
        }

        /// <inheritdoc/>
        public void AddRequestHandler<TRequest>(IRequestHandler<TRequest> requestHandler) 
            where TRequest : IRequest
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            foreach (var protocolListener in _protocolListeners)
            {
                protocolListener.AddRequestHandler<TRequest>(requestHandler);
            }

            ProtocolAdded += (sender, e) => {
                e.ProtocolListener.AddRequestHandler<TRequest>(requestHandler);
            };
        }

        /// <inheritdoc/>
        public void AddRequestHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> requestHandler)
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            foreach (var protocolListener in _protocolListeners)
            {
                protocolListener.AddRequestHandler<TRequest, TResponse>(requestHandler);
            }

            ProtocolAdded += (sender, e) => {
                e.ProtocolListener.AddRequestHandler<TRequest, TResponse>(requestHandler);
            };
        }

        /// <inheritdoc/>
        public void AddRequestHandlerAsync<TRequest, TResponse>(IRequestHandlerAsync<TRequest, TResponse> requestHandler)
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            foreach (var protocolListener in _protocolListeners)
            {
                protocolListener.AddRequestHandlerAsync<TRequest, TResponse>(requestHandler);
            }

            ProtocolAdded += (sender, e) => {
                e.ProtocolListener.AddRequestHandlerAsync<TRequest, TResponse>(requestHandler);
            };
        }


        public void Dispose()
        {
            if (Status == ServerStatus.Running || Status == ServerStatus.Starting)
            {
                Stop().Wait();
            }
        }
    }
}
