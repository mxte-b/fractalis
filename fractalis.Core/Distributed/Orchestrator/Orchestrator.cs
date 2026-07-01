using fractalis.Core.Compositor.Layers.Stylistic;
using fractalis.Core.Distributed.Clients;
using fractalis.Core.Distributed.Contexts;
using fractalis.Core.Distributed.Networking;
using fractalis.Core.Distributed.Networking.Messages;
using fractalis.Core.Distributed.Runtimes;
using fractalis.Core.Video;
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
        private const int BATCH_SIZE = 5;

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
        public Orchestrator(string serverUrl)
        {
            _dashboard.Initialize(Clients, serverUrl);
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
        /// Sends a message to a specified recipient.
        /// </summary>
        /// <param name="message">The message to send to the recipient.</param>
        /// <param name="recipientId">The unique identifier of the recipient.</param>
        /// <returns><see langword="true"/> if the message was delivered, <see langword="false"/> if the recipient could not be found.</returns>
        public async Task<bool> SendMessageAsync(Message message, Guid recipientId)
        {
            Clients.TryGetValue(recipientId, out var connection);
            if (connection is null) return false;

            await connection.SendMessageAsync(message);
            return true;
        }

        /// <summary>
        /// Splits a <see cref="RenderJob"/> into smaller assignments.
        /// </summary>
        /// <param name="job">The job to split.</param>
        private void SplitJobIntoAssignments(RenderJob job)
        {
            List<FrameRange> framesToRender = job.FramesToRender is not null
                ? job.FramesToRender
                : [new() {
                    Start = job.VideoConfig.StartFrame,
                    Count = job.VideoConfig.FrameCount
                }];

            foreach (var range in framesToRender)
            {
                foreach (var assignment in ChunkFrameRange(job.Id, range))
                {
                    Assignments.TryAdd(assignment.Id, assignment);
                }
            }
        }

        private static IEnumerable<RenderAssignment> ChunkFrameRange(Guid jobId, FrameRange range)
        {
            for (int i = range.Start; i <= range.End; i += BATCH_SIZE)
            {
                yield return new RenderAssignment()
                {
                    JobId = jobId,
                    StartFrameIndex = i,
                    FrameCount = Math.Min(BATCH_SIZE, range.End - i + 1)
                };
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
            // If the list is empty, it means that the video is already rendered, so just skip
            if (job.FramesToRender is not null && job.FramesToRender.Count == 0)
            {
                await SendMessageAsync(new RenderJobStatusMessage()
                {
                    JobId = job.Id,
                    Status = RenderStatus.Finished
                }, job.InitiatorId);
                return;
            }

            Jobs.TryAdd(job.Id, job);
            SplitJobIntoAssignments(job);
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
                Jobs.TryRemove(assignment.JobId, out _);

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
            ClientSessionRuntime runtime = new(this, connection);
            ConnectionCloseReason reason = await connection.ListenAsync(runtime, CancellationToken.None);

            // Handle connection closure
            if (reason != ConnectionCloseReason.NormalClosure)
            {
                _dashboard.AddLog(connection, "Disconnected unexpectedly.");
            }

            // Cancel any render jobs that were associated with the disconnected initiator.
            Clients.TryRemove(connection.Id, out _);
            if (connection.Role == ClientRole.Initiator)
            {
                var job = Jobs.FirstOrDefault(j => j.Value.InitiatorId == connection.Id);
                if (job.Value is not null) 
                {
                    Jobs.TryRemove(job.Key, out _);

                    var assignmentIdsToRemove = Assignments
                        .Where(a => a.Value.JobId == job.Key)
                        .Select(a => a.Key)
                        .ToList();

                    foreach (var assignmentId in assignmentIdsToRemove)
                    {
                        Assignments.TryRemove(assignmentId, out _);
                    }

                    await BroadcastMessageAsync(new RenderJobStatusMessage()
                    {
                        JobId = job.Value.Id,
                        Status = RenderStatus.Cancelled
                    }, ClientRole.Worker);
                }
            }
        }
    }
}