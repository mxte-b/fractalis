namespace fractalis.Core.Distributed.Clients
{
    /// <summary>
    /// Defines the role of a client in the system.
    /// </summary>
    public enum ClientRole
    {
        /// <summary>
        /// Client that initiates work.
        /// </summary>
        Initiator,

        /// <summary>
        /// Client that executes assigned tasks.
        /// </summary>
        Worker
    }
}
