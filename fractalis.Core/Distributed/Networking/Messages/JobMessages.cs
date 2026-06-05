using fractalis.Core.Renderers;
using fractalis.Core.Video;

namespace fractalis.Core.Distributed.Networking.Messages
{
    /// <summary>
    /// Message sent by the initiator to start rendering a video using distributed compute.
    /// </summary>
    public record VideoRenderRequest : Message
    {
        /// <summary>
        /// The URI where clients will upload the frames to.
        /// </summary>
        public required Uri                     UploadUri               { get; init; }

        /// <summary>
        /// Configuration of the video.
        /// </summary>
        public required VideoConfig             VideoConfig             { get; init; }

        /// <summary>
        /// Configuration of the fractal renderer.
        /// </summary>
        public required FractalRendererConfig   FractalRendererConfig   { get; init; }

    }

    /// <summary>
    /// Message sent by the orchestrator to a render client that gives all currently available render jobs.
    /// </summary>
    public record RenderJobListMessage : Message
    {
        public required List<RenderJob> Jobs { get; init; }
    }

    /// <summary>
    /// Message sent by the orchestrator to a render client when a new render job gets added.
    /// </summary>
    public record RenderJobAnnouncementMessage : Message
    {
        /// <summary>
        /// The added job.
        /// </summary>
        public required RenderJob Job { get; init; }
    }

    /// <summary>
    /// Message sent by the orchestrator to the initiator to report job status.
    /// </summary>
    public record RenderJobStatusMessage : Message
    {
        /// <summary>
        /// The unique identifier of the render job.
        /// </summary>
        public required Guid            JobId   { get; init; }

        /// <summary>
        /// The status of the render job.
        /// </summary>
        public required RenderStatus    Status  { get; init; }
    }
}
