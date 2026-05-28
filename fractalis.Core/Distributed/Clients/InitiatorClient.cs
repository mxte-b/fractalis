using fractalis.Core.Distributed.Runtimes;

namespace fractalis.Core.Distributed.Clients
{
    /// <summary>
    /// Represents an initiator client.
    /// </summary>
    public class InitiatorClient() : ClientWrapper<InitiatorRuntime>(ctx => new InitiatorRuntime(ctx))
    {
        /// <summary>
        /// Connects the initiator to the orchestrator.
        /// </summary>
        /// <param name="uri">The orchestrator URI.</param>
        /// <param name="displayName">The client display name.</param>
        public Task Connect(Uri uri, string displayName) => Connect(uri, displayName, ClientRole.Initiator);
    }
}
