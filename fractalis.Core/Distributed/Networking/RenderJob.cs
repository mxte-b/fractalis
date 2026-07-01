using fractalis.Core.Distributed.Networking.Messages;

namespace fractalis.Core.Distributed.Networking
{
    /// <summary>
    /// Represents a rendering job with associated configuration.
    /// </summary>
    public record RenderJob : VideoRenderRequest
    {
        /// <summary>
        /// Unique identifier of the job.
        /// </summary>
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <summary>
        /// The unique identifier of the initiator associated with this job.
        /// </summary>
        public required Guid InitiatorId { get; init; }
    }

    /// <summary>
    /// Defines the possible statuses a render job can have.
    /// </summary>
    public enum RenderStatus
    {
        /// <summary>
        /// Indicates that the render job is finished.
        /// </summary>
        Finished,

        /// <summary>
        /// Indicates that the render job is cancelled.
        /// </summary>
        Cancelled,

        /// <summary>
        /// Indicates that the render job has failed.
        /// </summary>
        Failed,
    }
}
