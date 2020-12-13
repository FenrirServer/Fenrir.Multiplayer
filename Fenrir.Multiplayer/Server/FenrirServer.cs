using Fenrir.Multiplayer.Exceptions;
using Fenrir.Multiplayer.LiteNet;
using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
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
        public string ServerId { get; set; }

        /// <inheritdoc/>
        public string Hostname { get; set; }

        /// <inheritdoc/>
        public IEnumerable<IProtocolListener> Listeners => _protocolListeners;

        /// <inheritdoc/>
        public ServerStatus Status { get; private set; } = ServerStatus.Stopped;

        /// <inheritdoc/>
        public bool IsRunning => Status == ServerStatus.Running;

        /// <summary>
        /// List of available protocols
        /// </summary>
        private IProtocolListener[] _protocolListeners;

        /// <summary>
        /// Fenrir Logger
        /// </summary>
        private IFenrirLogger _logger;

        /// <summary>
        /// Default constructor. Creates Fenrir Server with all default protocols 
        /// </summary>
        public FenrirServer()
            : this(new IProtocolListener[] { new LiteNetProtocolListener() })
        {
            _logger = new EventBasedLogger();
        }

        /// <summary>
        /// Creates Fenrir Server with specified protocols
        /// </summary>
        /// <param name="protocolListeners"></param>
        public FenrirServer(IProtocolListener[] protocolListeners)
        {
            ServerId = Guid.NewGuid().ToString();

            if(protocolListeners == null)
            {
                throw new ArgumentNullException(nameof(protocolListeners));
            }

            if (protocolListeners.Length == 0)
            {
                throw new ArgumentException("Protocol Listeners can not be empty", nameof(protocolListeners));
            }

            _protocolListeners = protocolListeners;
        }

        /// <inheritdoc/>
        public async Task Start()
        {
            if(Status != ServerStatus.Stopped)
            {
                throw new InvalidOperationException("Failed to start server, server is already " + Status);
            }

            Status = ServerStatus.Starting;

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

            Status = ServerStatus.Running;
        }

        /// <inheritdoc/>
        public async Task Stop()
        {
            if (Status != ServerStatus.Running && Status != ServerStatus.Starting)
            {
                throw new InvalidOperationException("Failed to stop server, server is already " + Status);
            }

            Status = ServerStatus.Stopping;

            // Stop all protocol listeners
            await Task.WhenAll(_protocolListeners.Select(listener => listener.Stop()));

            Status = ServerStatus.Stopped;
        }


        /// <inheritdoc/>
        public void SetLogger(IFenrirLogger logger)
        {
            foreach (var protocolListener in _protocolListeners)
            {
                protocolListener.SetLogger(logger);
            }
        }

        /// <inheritdoc/>
        public void SetContractSerializer(IContractSerializer contractSerializer)
        {
            foreach (var protocolListener in _protocolListeners)
            {
                protocolListener.SetContractSerializer(contractSerializer);
            }
        }
    }
}
