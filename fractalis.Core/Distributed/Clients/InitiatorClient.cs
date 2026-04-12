using fractalis.Core.Distributed.Runtimes;

namespace fractalis.Core.Distributed.Clients
{
    public class InitiatorClient : ClientWrapper<InitiatorRuntime>
    {
        public InitiatorClient() : base(ctx => new InitiatorRuntime(ctx)) { }

        public Task Connect(Uri uri, string displayName) => Connect(uri, displayName, ClientRole.Initiator);
    }
}
