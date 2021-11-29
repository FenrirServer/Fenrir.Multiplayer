namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Represents result of the connection attempt
    /// </summary>
    public class ConnectionResponse
    {
        /// <summary>
        /// Factory property, creates new successful connection result
        /// </summary>
        public static ConnectionResponse Successful => new ConnectionResponse(true, null);

        /// <summary>
        /// Factory method, creates new failed connection result with a given reason
        /// </summary>
        /// <param name="reason">Reason of the failure</param>
        /// <returns>New ClientConnectionResult with a given reason</returns>
        public static ConnectionResponse Failed(string reason) => new ConnectionResponse(false, reason);

        /// <summary>
        /// Indicates if connection attempt was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// If connection attempt was unsuccessful, contains reason for a failure
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Creates ConnectionResponse
        /// </summary>
        /// <param name="success">Indicates if connection attempt was successful</param>
        /// <param name="reason">Failure reason</param>
        public ConnectionResponse(bool success)
        {
            Success = success;
        }

        /// <summary>
        /// Creates ConnectionResponse
        /// </summary>
        /// <param name="success">Indicates if connection attempt was successful</param>
        /// <param name="reason">Failure reason</param>
        public ConnectionResponse(bool success, string reason)
        {
            Success = success;
            Reason = reason;
        }
    }
}
