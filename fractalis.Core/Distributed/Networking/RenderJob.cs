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
    }

    /// <summary>
    /// Defines the possible statuses a render job can have.
    /// </summary>
    public enum RenderJobStatus
    {
        /// <summary>
        /// Indicates that the render job is finished.
        /// </summary>
        Finished,

        /// <summary>
        /// Indicates that the render job is cancelled.
        /// </summary>
        Cancelled,
    }
}
