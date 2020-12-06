using Newtonsoft.Json.Linq;
using System;

namespace Fenrir.Multiplayer.Network
{
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
    }
}
