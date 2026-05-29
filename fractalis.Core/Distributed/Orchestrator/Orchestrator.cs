using fractalis.Core.Distributed.Clients;
using fractalis.Core.Distributed.Contexts;
using fractalis.Core.Distributed.Networking;
using fractalis.Core.Distributed.Networking.Messages;
using fractalis.Core.Distributed.Runtimes;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace fractalis.Core.Distributed.Orchestrator
{
    /// <summary>
    /// Manages connected clients, message broadcasting, and orchestrator-level operations.
    /// </summary>
    /// <remarks>
    /// Integrates with <see cref="OrchestratorDashboard"/> to provide live console visualization
    /// of clients and logs. Handles client registration, connection lifecycle, and incoming messages.
    /// </remarks>
    public class Orchestrator : IOrchestratorContext
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
        /// Active render jobs keyed by <see cref="RenderJob.Id"/>.
        /// </summary>
        public ConcurrentDictionary<Guid, RenderJob>        Jobs                = new();

        /// <summary>
        /// Active assignments keyed by <see cref="RenderAssignment.Id"/>. Removing an entry signals completion.
        /// </summary>
        public ConcurrentDictionary<Guid, RenderAssignment> Assignments         = new();

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
        public async Task BroadcastMessageAsync(Message message, ClientRole? targetRole = null)
        {
            var tasks = Clients.Values.Where(c => targetRole == null || c.Role == targetRole).Select(c => c.SendMessageAsync(message));
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Splits a <see cref="RenderJob"/> into smaller assignments.
        /// </summary>
        /// <param name="job">The job to split.</param>
        /// <param name="size">Maximum number of frames per assignment.</param>
        private void SplitJobIntoAssignments(RenderJob job, int size)
        {
            for (int i = job.VideoConfig.StartFrame; i < job.VideoConfig.FrameCount; i += size)
            {
                RenderAssignment assignment = new()
                {
                    JobId = job.Id,
                    StartFrameIndex = i,
                    FrameCount = Math.Min(size, job.VideoConfig.FrameCount - i)
                };

                Assignments.TryAdd(assignment.Id, assignment);
            }
        }

        private static async Task<bool> IsHealthy(string url)
        {
            try
            {
                await HttpHelper.GetAsync(url);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region Context-revealed methods
        /// <inheritdoc/>
        public async Task AddJobAsync(RenderJob job)
        {
            Jobs.TryAdd(job.Id, job);
            SplitJobIntoAssignments(job, 5);
            await BroadcastMessageAsync(new RenderJobAnnouncementMessage() { Job = job }, ClientRole.Worker);
        }

        /// <inheritdoc/>
        public Message GetRenderAssignment()
        {
            RenderAssignment? assignment = Assignments.Values.FirstOrDefault(x => x.TryClaim());
            if (assignment is null) return new NoAssignmentMessage();

            return new RenderAssignmentMessage() { Assignment = assignment };
        }

        /// <inheritdoc/>
        public void CompleteAssignment(Guid assignmentId)
        {
            Assignments.TryRemove(assignmentId, out RenderAssignment? assignment);
            if (assignment is null) return;

            if (!Assignments.Any(a => a.Value.JobId == assignment?.JobId))
            {
                _ = BroadcastMessageAsync(new RenderJobStatusMessage() 
                { 
                    JobId = assignment.JobId, 
                    Status = RenderStatus.Finished 
                });
            }

            return;
        }

        /// <inheritdoc/>
        public void CancelAssignment(Guid assignmentId)
        {
            Assignments.TryGetValue(assignmentId, out RenderAssignment? assignment);
            assignment?.TryCancel();
        }

        /// <inheritdoc/>
        public void Log(string message) => _dashboard.AddLog(message);

        /// <inheritdoc/>
        public void Log(ClientConnection connection, string message) => _dashboard.AddLog(connection, message);
        #endregion

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
                await connection.SendMessageAsync(new RenderJobListMessage() { Jobs = Jobs.Values.ToList() });
            }

            // Listen for incoming messages from the client
            ClientSessionRuntime runtime = new ClientSessionRuntime(this, connection);
            ConnectionCloseReason reason = await connection.ListenAsync(runtime, CancellationToken.None);

            if (reason != ConnectionCloseReason.NormalClosure)
            {
                _dashboard.AddLog(connection, "Disconnected unexpectedly.");
            }

            Clients.TryRemove(connection.Id, out _);
        }
    }
}