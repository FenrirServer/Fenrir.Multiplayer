using Newtonsoft.Json.Linq;
using System;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Serializable structure that describes
    /// available server protocol
    /// </summary>
    public class ProtocolInfo
    {
        /// <summary>
        /// Type of the supported protocol
        /// </summary>
        public ProtocolType ProtocolType { get; set; }

        /// <summary>
        /// Protocol-specific connection data
        /// </summary>
        public JObject ConnectionData { get; set; }

        /// <summary>
        /// Returns the connection data for a given data type
        /// </summary>
        /// <param name="connectionDataType">Type of the connection data</param>
        /// <returns></returns>
        public object GetConnectionData(Type connectionDataType)
        {
            if(ConnectionData == null)
            {
                return null;
            }

            return ConnectionData.ToObject(connectionDataType);
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ProtocolInfo()
        {
        }

        /// <summary>
        /// Constructs an object using custom protocol type and connection data
        /// </summary>
        /// <param name="protocolType">Type of the protocol</param>
        /// <param name="connectionData">Connection data</param>
        public ProtocolInfo(ProtocolType protocolType, IProtocolConnectionData connectionData) : this()
        {
            ProtocolType = protocolType;
            ConnectionData = JObject.FromObject(connectionData);
        }
    }
}
