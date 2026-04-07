using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace fractalis.Core.Distributed
{
    /// <summary>
    /// Manages connected clients, message broadcasting, and orchestrator-level operations.
    /// </summary>
    /// <remarks>
    /// Integrates with <see cref="OrchestratorDashboard"/> to provide live console visualization
    /// of clients and logs. Handles client registration, connection lifecycle, and incoming messages.
    /// </remarks>
    public class Orchestrator
    {
        private readonly OrchestratorDashboard              _dashboard          = OrchestratorDashboard.Instance;

        /// <summary>
        /// Maximum duration to wait for a client to register before timing out.
        /// </summary>
        public readonly static TimeSpan                     RegistrationTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Thread-safe dictionary of connected clients keyed by <see cref="Guid"/>.
        /// </summary>
        public ConcurrentDictionary<Guid, ClientConnection> Clients             = new();

        /// <summary>
        /// Collection of active or queued rendering jobs.
        /// </summary>
        public ConcurrentBag<RenderJob>                     Jobs                = [];

        /// <summary>
        /// Initializes the orchestrator and starts the dashboard UI.
        /// </summary>
        public Orchestrator()
        {
            _dashboard.Initialize(Clients);
            _dashboard.Start();
        }

        /// <summary>
        /// Broadcasts a message to all currently connected clients, optionally filtered by role.
        /// </summary>
        /// <param name="message">The <see cref="Message"/> to send.</param>
        /// <param name="targetRole">
        /// If specified, only clients with this <see cref="ClientRole"/> will receive the message.
        /// If <see langword="null"/>, the message is sent to all connected clients.
        /// </param>
        /// <returns>A <see cref="Task"/> that completes when all sends have finished.</returns>
        public async Task BroadcastMessage(Message message, ClientRole? targetRole = null)
        {
            var tasks = Clients.Values.Where(c => targetRole == null || c.Role == targetRole).Select(c => c.SendMessageAsync(message));
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Handles a new client connection, including registration, message handling, and disconnection.
        /// </summary>
        /// <param name="webSocket">The WebSocket representing the client connection.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous handling operation.</returns>
        public async Task HandleClient(WebSocket webSocket)
        {
            _dashboard.AddLog("New client connected!");

            // Wait for client registration
            ClientConnection? connection = await ClientConnection.NegotiateAsync(webSocket, RegistrationTimeout);
            if (connection is null)
            {
                _dashboard.AddLog("Client registration timed out.");
                return;
            }

            // Register the connection
            Clients.TryAdd(connection.Id, connection);
            _dashboard.AddLog(connection, "Registered.");

            // Send available jobs to the client (if a worker)
            if (connection.Role == ClientRole.Worker)
            {
                await connection.SendMessageAsync(new RenderJobListMessage() { Jobs = Jobs.ToList() });
            }

            // Listen for incoming messages from the client
            ConnectionCloseReason reason = await connection.ListenAsync(async (message, _) =>
            {
                // Logging
                if (message == null)
                {
                    _dashboard.AddLog(connection, "Received invalid message");
                }
                _dashboard.AddLog(connection, message is null ?  "No content" : message.ToString());

                // Message handling
                switch (message)
                {
                    case VideoRenderRequest renderRequest:
                        RenderJob job = new RenderJob()
                        {
                            VideoConfig = renderRequest.VideoConfig,
                            FractalRendererConfig = renderRequest.FractalRendererConfig,
                        };

                        Jobs.Add(job);
                        await BroadcastMessage(new RenderJobAnnouncementMessage() { Job = job }, ClientRole.Worker);
                        break;
                }

                return MessageHandlingResult.Continue;
            }, CancellationToken.None);

            if (reason != ConnectionCloseReason.NormalClosure)
            {
                _dashboard.AddLog(connection, "Disconnected unexpectedly.");
            }

            Clients.TryRemove(connection.Id, out _);
        }
    }
}