namespace Fenrir.Multiplayer.Network
{
    public enum MessageDeliveryMethod : byte
    {
        /// <summary>
        /// Reliable. Packets won't be dropped, won't be duplicated, can arrive in no specific order.
        /// </summary>
        ReliableUnordered = 0,

        /// <summary>
        /// Unreliable. Packets can be dropped, won't be duplicated, will arrive in order.
        /// </summary>
        Sequenced = 1,

        /// <summary>
        /// Reliable and ordered. Packets won't be dropped, won't be duplicated, will arrive in order.
        /// </summary>
        ReliableOrdered = 2,

        /// <summary>
        /// Only last packet is reliable. Packets can be dropped (except the last one), won't
        //  be duplicated, will arrive in order.
        /// </summary>
        ReliableSequenced = 3,

        /// <summary>
        /// Unreliable. Packets can be dropped, can be duplicated, can arrive in no specific order.
        /// </summary>
        Unreliable = 4
    }
}
