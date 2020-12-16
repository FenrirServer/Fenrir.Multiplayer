namespace Fenrir.Multiplayer.Serialization
{
    /// <summary>
    /// Indicates that class can be reset and re-cycled to be re-used later
    /// </summary>
    interface IRecyclable
    {
        /// <summary>
        /// Resets the state of the object,
        /// making it possible to re-use later
        /// </summary>
        void Recycle();
    }
}
