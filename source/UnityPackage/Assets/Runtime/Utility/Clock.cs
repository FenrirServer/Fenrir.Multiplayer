using System;
using System.Diagnostics;

namespace Fenrir.Multiplayer
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
        public DateTime UtcNow => _startTimeUtc.Add(_stopWatch.Elapsed) + Offset;

        /// <summary>
        /// Time in UTC without offset
        /// </summary>
        public DateTime UtcNowRaw => _startTimeUtc.Add(_stopWatch.Elapsed);

        /// <summary>
        /// Local time now
        /// </summary>
        public DateTime Now => _startTime.Add(_stopWatch.Elapsed) + Offset;

        /// <summary>
        /// Local time now
        /// </summary>
        public DateTime NowRaw => _startTime.Add(_stopWatch.Elapsed);

        /// <summary>
        /// Starting time
        /// </summary>
        private DateTime _startTime = DateTime.Now;

        /// <summary>
        /// Starting utc time
        /// </summary>
        private DateTime _startTimeUtc = DateTime.UtcNow;

        /// <summary>
        /// Stopwatch to track high-resolution time 
        /// </summary>
        private Stopwatch _stopWatch = new Stopwatch();

        /// <summary>
        /// Constructor
        /// </summary>
        public Clock()
        {
            _stopWatch.Start();
        }
    }
}
