using Fenrir.Multiplayer.Rooms;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fenrir.Multiplayer.Sim
{
    /// <summary>
    /// Indicates a result of simulation join operation
    /// </summary>
    public class SimulationJoinResult
    {
        /// <summary>
        /// Result of the underlying room join operation
        /// </summary>
        public RoomJoinResponse Response { get; private set; }

        /// <summary>
        /// Indicates if operation completed successfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// If requested operation was not executed successfully, might contain a numeric error code to pass to the client
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// If requested operation was not executed successfully, might contain text description of the reason
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Creates a simulation join result
        /// </summary>
        /// <param name="roomJoinResponse">Result of room join response operation</param>
        public SimulationJoinResult(RoomJoinResponse roomJoinResponse)
        {
            Success = roomJoinResponse.Success;
            ErrorCode = roomJoinResponse.ErrorCode;
            Reason = roomJoinResponse.Reason;
        }

        /// <summary>
        /// Creates failed simulation join result
        /// </summary>
        /// <param name="errorCode">Error code</param>
        /// <param name="reason">Failure reason</param>
        public SimulationJoinResult(int errorCode, string reason)
        {
            Success = false;
            ErrorCode = errorCode;
            Reason = reason;
        }
    }
}
