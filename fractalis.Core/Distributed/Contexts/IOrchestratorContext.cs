using fractalis.Core.Distributed.Networking;

namespace fractalis.Core.Distributed.Contexts
{
    /// <summary>
    /// Provides a communication and control context for orchestrator-side execution logic.
    /// </summary>
    public interface IOrchestratorContext
    {
        /// <summary>
        /// Adds a new render job to the orchestrator queue.
        /// </summary>
        /// <param name="job">The render job to enqueue.</param>
        Task AddJobAsync(RenderJob job);

        /// <summary>
        /// Retrieves a render assignment for a client to process.
        /// </summary>
        /// <returns>A message describing the render assignment.</returns>
        Message GetRenderAssignment();

        /// <summary>
        /// Marks a render assignment as completed.
        /// </summary>
        /// <param name="assignmentId">The unique identifier of the assignment.</param>
        void CompleteAssignment(Guid assignmentId);

        /// <summary>
        /// Cancels an active render assignment.
        /// </summary>
        /// <param name="assignmentId">The unique identifier of the assignment.</param>
        void CancelAssignment(Guid assignmentId);

        /// <summary>
        /// Writes a log message to the orchestrator log system.</summary>
        /// <param name="message">The message to log.</param>
        void Log(string message);

        /// <summary>
        /// Writes a log message associated with a specific client connection.</summary>
        /// <param name="connection">The client connection that generated the log entry.</param>
        /// <param name="message">The message to log.</param>
        void Log(ClientConnection connection, string message);
    }
}
