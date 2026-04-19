using fractalis.Core.Distributed.Contexts;
using fractalis.Core.Distributed.Networking;
using fractalis.Core.Distributed.Runtimes;

namespace fractalis.Core.Distributed.Clients
{
    public class ClientWrapper<TRuntime> : IClientContext
        where TRuntime : IRuntime
    {
        private readonly Client     _client;
        private readonly TRuntime   _runtime;

        public bool Connected => _client.Connected;

        public ClientWrapper(Func<IClientContext, TRuntime> runtimeFactory)
        {
            _runtime = runtimeFactory(this);
            _client = new Client(_runtime);
        }

        #region Wrapped methods
        protected Task Connect(Uri uri, string displayName, ClientRole role = ClientRole.Worker) => _client.Connect(uri, displayName, role);

        public Task Start() => _client.Start();

        public Task Disconnect() => _client.Disconnect();

        public Task SendMessageToServerAsync(Message message) => _client.SendMessageToServerAsync(message);
        #endregion
    }
}
