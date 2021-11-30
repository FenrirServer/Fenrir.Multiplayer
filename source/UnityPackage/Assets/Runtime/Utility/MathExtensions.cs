using System;

namespace Fenrir.Multiplayer.Utility
{
    static partial class MathExtensions
    {
        /// <summary>
        /// Moves value towards the target
        /// </summary>
        /// <param name="current">Current value</param>
        /// <param name="target">Target value</param>
        /// <param name="maxDelta">Maximum step</param>
        /// <returns>Resulting value</returns>
        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (Math.Abs(target - current) <= maxDelta)
            {
                return target;
            }

            return current + Math.Sign(target - current) * maxDelta;
        }

        /// <summary>
        /// Moves value towards the target
        /// </summary>
        /// <param name="current">Current value</param>
        /// <param name="target">Target value</param>
        /// <param name="maxDelta">Maximum step</param>
        /// <returns>Resulting value</returns>
        public static double MoveTowards(double current, double target, double maxDelta)
        {
            if (Math.Abs(target - current) <= maxDelta)
            {
                return target;
            }

            return current + Math.Sign(target - current) * maxDelta;
        }
    }
}
