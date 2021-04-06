namespace Fenrir.Multiplayer.Utility
{
    /// <summary>
    /// Calculates deterministic hash for a string using fnv-1 algorithm
    /// This is useful when transfering Type or MemberInfo information over the network
    /// </summary>
    static class DeterministicHashUtility
    {
        /// <summary>
        /// Calculates deterministic member name hash using fnv-1
        /// </summary>
        /// <param name="str">String to calculate hash for</param>
        /// <returns>Deterministic string hash</returns>
        public static ulong CalculateHash(string str)
        {
            // Calculates fnv-1 64 bit hash of the type name

            ulong hash = 14695981039346656037UL; // Offset

            for (var i = 0; i < str.Length; i++)
            {
                hash = hash ^ str[i];
                hash *= 1099511628211UL; // Prime
            }

            return hash;
        }
    }
}
