namespace Fenrir.Multiplayer.Rooms
{
    /// <summary>
    /// Base class for a Fenrir Multiplayer Room.
    /// Rooms allow you to build an isolated layer of gameplay and
    /// business logic using single-threaded event loop, and benefit
    /// from multi-threaded architecture where each server can handle thousands of players.
    /// </summary>
    public abstract class BaseRoom : IRoom
    {
        /// <summary>
        /// Room action queue
        /// </summary>
        private ActionQueue _actionQueue;
    }
}
