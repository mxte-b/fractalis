using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed
{
    /// <summary>
    /// Represents a client that connects to the orchestrator server to send and receive messages.
    /// </summary>
    /// <remarks>
    /// Manages connection registration, message listening, sending, and graceful disconnection.
    /// Uses <see cref="ClientRuntime"/> to handle incoming messages.
    /// </remarks>
    public class RenderClient
    {
        private ServerConnection?           _connection;
        private readonly ClientRuntime      _runtime = new ClientRuntime();
        private static readonly TimeSpan    RegistrationTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets a value indicating whether the client is currently connected to the server.
        /// </summary>
        public bool Connected { get; private set; } = false;

        /// <summary>
        /// Connects the client to the specified server URI and registers with the given display name.
        /// </summary>
        /// <param name="uri">The server URI to connect to.</param>
        /// <param name="displayName">The display name to register with the server.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous connect operation.</returns>
        /// <remarks>
        /// If the WebSocket fails to open or registration fails, <see cref="Connected"/> remains false.
        /// </remarks>
        public async Task Connect(Uri uri, string displayName)
        {
            ClientWebSocket ws = new ClientWebSocket();

            await ws.ConnectAsync(uri, default);
            if (ws.State != WebSocketState.Open)
            {
                return;
            }

            _connection = await ServerConnection.RegisterAsync(displayName, ws, RegistrationTimeout);
            if (_connection != null)
            {
                Connected = true;
            }
        }

        /// <summary>
        /// Starts listening for incoming messages from the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous listen operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the client is not connected when calling this method.
        /// </exception>
        public async Task Start()
        {
            if (!Connected)
            {
                throw new InvalidOperationException("Cannot start client because it is not connected.");
            }

            await _connection!.ListenAsync(async (message, _) =>
            {
                if (message != null)
                {
                    await _runtime.HandleMessage(message);
                }

                return MessageHandlingResult.Continue;
            }, CancellationToken.None);
        }

        /// <summary>
        /// Sends a <see cref="Message"/> to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous send operation.</returns>
        /// <remarks>
        /// Does nothing if the client is not connected.
        /// </remarks>
        public async Task SendMessageToServerAsync(Message message)
        {
            if (!Connected) return;

            await _connection!.SendMessageAsync(message);
        }

        /// <summary>
        /// Disconnects the client from the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous disconnect operation.</returns>
        /// <remarks>
        /// Does nothing if the client is already disconnected.
        /// </remarks>
        public async Task Disconnect()
        {
            if (!Connected) return;

            await _connection!.Close();
            Connected = false;
        }
    }
}