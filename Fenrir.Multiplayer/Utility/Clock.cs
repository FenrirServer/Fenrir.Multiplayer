using System;

namespace Fenrir.Multiplayer.Utility
{
    /// <summary>
    /// Network clock that allows synchronization between
    /// two peers
    /// </summary>
    public class Clock
    {
        /// <summary>
        /// Clock offset from DateTime. 
        /// This can be adjusted to correct clock from drifting
        /// </summary>
        public TimeSpan Offset { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Time in UTC
        /// </summary>
        public DateTime UtcNow => DateTime.UtcNow + Offset;

        /// <summary>
        /// Local time now
        /// </summary>
        public DateTime Now => DateTime.Now + Offset;

        public Clock()
        {
        }
    }
}
