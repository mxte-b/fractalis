namespace fractalis.Core.Distributed.Networking
{
    /// <summary>
    /// Represents the reason why a WebSocket connection was closed.
    /// </summary>
    public enum ConnectionCloseReason
    {
        /// <summary>
        /// The connection closed normally without errors or interruptions.
        /// </summary>
        NormalClosure,

        /// <summary>
        /// The connection was cancelled, for example due to a timeout.
        /// </summary>
        Cancelled,

        /// <summary>
        /// The connection was closed due to an error or unexpected failure.
        /// </summary>
        Error
    }
}
