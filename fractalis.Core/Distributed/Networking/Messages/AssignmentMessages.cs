namespace fractalis.Core.Distributed.Networking.Messages
{
    /// <summary>
    /// Message sent by a worker to the orchestrator to request a render job.
    /// </summary>
    public record RenderAssignmentRequest : Message;

    /// <summary>
    /// Message sent by the orchestrator to a render client for rendering images.
    /// </summary>
    public record RenderAssignmentMessage : Message
    {
        /// <summary>
        /// The assignment.
        /// </summary>
        public required RenderAssignment    Assignment     { get; init; }
    }

    /// <summary>
    /// Message sent by the orchestrator to a render client when no assignment could be provided.
    /// </summary>
    public record NoAssignmentMessage : Message;

    /// <summary>
    /// Message representing a status update for a render assignment.
    /// </summary>
    public record RenderAssignmentStatusMessage : Message
    {
        /// <summary>
        /// The unique identifier of the assignment.
        /// </summary>
        public required Guid                AssignmentId    { get; init; }

        /// <summary>
        /// Current status of the assignment.
        /// </summary>
        public required RenderStatus        Status          { get; init; }
    }
}
