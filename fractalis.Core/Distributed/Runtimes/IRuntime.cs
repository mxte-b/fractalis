using fractalis.Core.Distributed.Networking;

namespace fractalis.Core.Distributed.Runtimes
{
    /// <summary>
    /// Handles incoming messages and rendering for a client instance.
    /// </summary>
    public interface IRuntime
    {
        /// <summary>
        /// Processes an incoming <see cref="Message"/> and performs the appropriate action.
        /// </summary>
        /// <param name="message">The message to handle. Must not be <see langword="null"/>.</param>
        /// <returns>
        /// A <see cref="MessageHandlingResult"/> describing the outcome of the operation.
        /// </returns>
        public Task<MessageHandlingResult> HandleMessage(Message message);
    }
}