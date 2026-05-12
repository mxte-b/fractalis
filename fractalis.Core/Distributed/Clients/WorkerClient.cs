using fractalis.Core.Distributed.Runtimes;

namespace fractalis.Core.Distributed.Clients
{
    public record WorkerClientOptions
    {
        public static WorkerClientOptions Default { get; } = new WorkerClientOptions();
        public double ProcessorUsageLimit { get; init; } = 1;
    }

    public class WorkerClient(WorkerClientOptions? options = null)
        : ClientWrapper<WorkerRuntime>(ctx => new WorkerRuntime(ctx, options ?? WorkerClientOptions.Default))
    {
        public Task Connect(Uri uri, string displayName) => Connect(uri, displayName, ClientRole.Worker);
    }
}
