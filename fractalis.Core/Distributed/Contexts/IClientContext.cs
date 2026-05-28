using fractalis.Core.Distributed.Networking;

namespace fractalis.Core.Distributed.Contexts
{
    /// <summary>
    /// Represents a client execution context capable of communicating with the orchestrator.
    /// </summary>
    /// <remarks>
    /// Implementations provide access to core messaging functionality used by runtime
    /// components to send messages back to the orchestrator.
    /// </remarks>
    public interface IClientContext
    {
        /// <summary>
        /// Sends a message asynchronously to the orchestrator.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        Task SendMessageToServerAsync(Message message);
    }
}
