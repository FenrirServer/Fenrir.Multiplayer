using System;
using System.Collections.Generic;
using System.Text;

namespace Fenrir.Multiplayer.Utility
{
    static class TimeSpanExtensions
    {
        /// <summary>
        /// Moves value towards the target
        /// </summary>
        /// <param name="current">Current value</param>
        /// <param name="target">Target value</param>
        /// <param name="maxDelta">Maximum step</param>
        /// <returns>Resulting value</returns>
        public static TimeSpan MoveTowards(TimeSpan current, TimeSpan target, TimeSpan maxDelta)
        {
            if (Math.Abs(target.TotalMilliseconds - current.TotalMilliseconds) <= maxDelta.TotalMilliseconds)
            {
                return target;
            }

            return TimeSpan.FromMilliseconds(current.TotalMilliseconds + Math.Sign(target.TotalMilliseconds - current.TotalMilliseconds) * maxDelta.TotalMilliseconds);
        }
    }
}
