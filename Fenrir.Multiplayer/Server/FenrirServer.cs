using Fenrir.Multiplayer.Network;
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

        /// <summary>
        /// List of available protocols
        /// </summary>
        private List<IProtocolListener> _protocolListeners = new List<IProtocolListener>();

        /// <inheritdoc/>
        public IEnumerable<IProtocolListener> Listeners => _protocolListeners;

        /// <inheritdoc/>
        public ServerStatus Status { get; private set; } = ServerStatus.Stopped;

        /// <summary>
        /// Default constructor
        /// </summary>
        public FenrirServer()
        {
            ServerId = Guid.NewGuid().ToString();
        }

        /// <inheritdoc/>
        public void AddProtocol(IProtocol protocol)
        {
            IProtocolListener protocolListener = protocol.CreateListener();
            _protocolListeners.Add(protocolListener);
        }

        /// <inheritdoc/>
        public async Task Start()
        {
            if(Status != ServerStatus.Stopped)
            {
                throw new InvalidOperationException("Failed to start server, server is already " + Status);
            }

            // Start all protocol listeners
            await Task.WhenAll(_protocolListeners.Select(listener => listener.Start()));
        }

        /// <inheritdoc/>
        public async Task Stop()
        {
            if (Status != ServerStatus.Running && Status != ServerStatus.Starting)
            {
                throw new InvalidOperationException("Failed to stop server, server is already " + Status);
            }

            // Stop all protocol listeners
            await Task.WhenAll(_protocolListeners.Select(listener => listener.Stop()));
        }
    }
}
