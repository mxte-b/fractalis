using fractalis.Core.Distributed.Runtimes;

namespace fractalis.Core.Distributed.Clients
{
    /// <summary>
    /// Initializes a new worker client instance.
    /// </summary>
    /// <param name="processorUsageLimit">Maximum allowed CPU usage.</param>
    public class WorkerClient(double processorUsageLimit = 1)
        : ClientWrapper<WorkerRuntime>(ctx => new WorkerRuntime(ctx, processorUsageLimit))
    {
        /// <summary>
        /// Connects the initiator to the orchestrator.
        /// </summary>
        /// <param name="uri">The orchestrator URI.</param>
        /// <param name="displayName">The client display name.</param>
        public Task Connect(Uri uri, string displayName) => Connect(uri, displayName, ClientRole.Worker);
    }
}
