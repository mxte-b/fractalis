namespace fractalis.Core.Distributed.Networking
{
    /// <summary>
    /// Represents the outcome of processing a message.
    /// </summary>
    public enum MessageHandlingResult
    {
        /// <summary>
        /// Indicates that message listening should continue normally.
        /// </summary>
        Continue,

        /// <summary>
        /// Indicates that message listening should stop.
        /// </summary>
        Stop
    }
}
