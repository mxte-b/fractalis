using fractalis.Core.Distributed.Contexts;
using fractalis.Core.Distributed.Networking;
using fractalis.Core.Distributed.Runtimes;

namespace fractalis.Core.Distributed.Clients
{
    /// <summary>
    /// Wraps a <see cref="Client"/> instance and exposes a runtime-aware client context.
    /// </summary>
    /// <typeparam name="TRuntime">
    /// The runtime type used by the client.
    /// </typeparam>
    public class ClientWrapper<TRuntime> : IClientContext
        where TRuntime : IRuntime
    {
        private readonly Client     _client;
        private readonly TRuntime   _runtime;

        /// <summary>Gets whether the client is currently connected.</summary>
        public bool Connected => _client.Connected;

        /// <summary>
        /// Initializes a new client wrapper instance.
        /// </summary>
        /// <param name="runtimeFactory">
        /// Factory used to create the runtime instance.
        /// </param>
        public ClientWrapper(Func<IClientContext, TRuntime> runtimeFactory)
        {
            _runtime = runtimeFactory(this);
            _client = new Client(_runtime);
        }

        #region Wrapped methods

        /// <summary>
        /// Connects the client to the orchcestrator.
        /// </summary>
        /// <param name="uri">The URI of the orchcestrator.</param>
        /// <param name="displayName">The client display name.</param>
        /// <param name="role">
        /// The role assigned to the client.
        /// </param>
        public Task Connect(Uri uri, string displayName, ClientRole role = ClientRole.Worker) => _client.Connect(uri, displayName, role);

        /// <summary>
        /// Starts the client runtime.
        /// </summary>
        public Task Start() => _client.Start();

        /// <summary>
        /// Disconnects the client.
        /// </summary>
        public Task Disconnect() => _client.Disconnect();

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public Task SendMessageToServerAsync(Message message) => _client.SendMessageToServerAsync(message);
        #endregion
    }
}
