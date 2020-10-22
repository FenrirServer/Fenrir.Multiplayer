namespace Fenrir.Multiplayer.Network
{
    public class ConnectionResult
    {
        public static ConnectionResult Successful => new ConnectionResult(true, null);

        public static ConnectionResult Failed(string reason) => new ConnectionResult(false, reason);

        public bool Success { get; set; }

        public string Reason { get; set; }

        public ConnectionResult(bool success, string reason)
        {
            Success = success;
            Reason = reason;
        }
    }
}
