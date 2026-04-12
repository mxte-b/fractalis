using fractalis.Core.Distributed.Runtimes;

namespace fractalis.Core.Distributed.Clients
{
    public class WorkerClient : ClientWrapper<WorkerRuntime>
    {
        public WorkerClient() : base(ctx => new WorkerRuntime(ctx)) { }

        public Task Connect(Uri uri, string displayName) => Connect(uri, displayName, ClientRole.Worker);
    }
}
