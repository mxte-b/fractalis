using fractalis.Core.Distributed.Networking;

namespace fractalis.Core.Distributed.Runtimes
{
    /// <summary>
    /// Handles incoming messages and rendering for a client instance.
    /// </summary>
    public interface IRuntime
    {
        /// <summary>
        /// Processes an incoming <see cref="Message"/> by serializing it to the console.
        /// </summary>
        /// <param name="message">The message to handle. Must not be <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<MessageHandlingResult> HandleMessage(Message message);
    }
}