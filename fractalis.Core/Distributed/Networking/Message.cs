using fractalis.Core.Distributed.Clients;
using fractalis.Core.Video;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace fractalis.Core.Distributed.Networking
{
    /// <summary>
    /// Base class for all message types exchanged between clients and the orchestrator.
    /// </summary>
    /// <remarks>
    /// Uses JSON polymorphic serialization with a type discriminator property named "type".
    /// </remarks>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(DebugMessage), "debug")]
    [JsonDerivedType(typeof(ReconnectMessage), "reconnect")]
    [JsonDerivedType(typeof(RenderAssignmentRequest), "jobRequest")]
    [JsonDerivedType(typeof(RenderJobListMessage), "jobList")]
    [JsonDerivedType(typeof(NoAssignmentMessage), "noAssignment")]
    [JsonDerivedType(typeof(RegistrationMessage), "registration")]
    [JsonDerivedType(typeof(VideoRenderRequest), "renderRequest")]
    [JsonDerivedType(typeof(RenderJobStatusMessage), "jobStatus")]
    [JsonDerivedType(typeof(RenderedImageMessage), "renderedImage")]
    [JsonDerivedType(typeof(RenderAssignmentMessage), "jobAssignment")]
    [JsonDerivedType(typeof(RenderJobAnnouncementMessage), "jobAnnouncement")]
    [JsonDerivedType(typeof(RegistrationAcknowledgedMessage), "registrationAcknowledged")]
    public abstract record Message;

    #region Message types
    /// <summary>
    /// Message sent by a client to register itself with the orchestrator.
    /// </summary>
    public record RegistrationMessage : Message
    {
        /// <summary>
        /// The display name of the client for identification purposes.
        /// </summary>
        public required string          DisplayName { get; init; }

        public ClientRole               Role        { get; init; } = ClientRole.Worker;
    }

    /// <summary>
    /// Message sent by a client to reconnect using its existing client ID.
    /// </summary>
    public record ReconnectMessage : Message
    {
        /// <summary>
        /// The unique identifier of the client attempting to reconnect.
        /// </summary>
        public required Guid            ClientId    { get; init; }
    }

    /// <summary>
    /// Message sent by the orchestrator to acknowledge a client's registration.
    /// </summary>
    public record RegistrationAcknowledgedMessage : Message
    {
        /// <summary>
        /// The unique identifier assigned to the client by the orchestrator.
        /// </summary>
        public required Guid            ClientId    { get; init; }
    }

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
        public required List<RenderJob>         Jobs                    { get; init; }   
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
        public required RenderAssignment Assignment { get; init; }
    }

    /// <summary>
    /// Message sent by the orchestrator to a render client when no assignment could be provided.
    /// </summary>
    public record NoAssignmentMessage : Message;

    /// <summary>
    /// Message sent by the orchestrator to the initiator to report job status.
    /// </summary>
    public record RenderJobStatusMessage : Message
    {
        /// <summary>
        /// The unique identifier of the render job.
        /// </summary>
        public required Guid            RenderJobId     { get; init; }

        /// <summary>
        /// The status of the render job.
        /// </summary>
        public required RenderJobStatus Status          { get; init; }
    }

    /// <summary>
    /// Message containing rendered image data for a specific frame.
    /// </summary>
    public record RenderedImageMessage : Message
    {
        /// <summary>
        /// Index of the rendered frame.
        /// </summary>
        public required int             FrameIndex      { get; init; }

        /// <summary>
        /// Raw image data in byte form.
        /// </summary>
        public required byte[]          Bytes           { get; init; }
    }

    /// <summary>
    /// Message used for debugging purposes, containing arbitrary text content.
    /// </summary>
    public record DebugMessage : Message
    {
        /// <summary>
        /// The debug message content.
        /// </summary>
        public required string          Content     { get; init; }
    }
    #endregion

    /// <summary>
    /// Provides serialization utilities for <see cref="Message"/> objects.
    /// </summary>
    public static class MessageSerializer
    {
        private static readonly JsonSerializerOptions _options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        /// <summary>
        /// Serializes a <see cref="Message"/> into a UTF-8 encoded byte array.
        /// </summary>
        /// <param name="message">The message to serialize. Must not be <see langword="null"/>.</param>
        /// <returns>A UTF-8 encoded byte array representing the JSON-serialized message.</returns>
        public static byte[] Serialize(Message message)
        {
            string text = JsonSerializer.Serialize(message, _options);
            return Encoding.UTF8.GetBytes(text);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="Message"/> instance.
        /// </summary>
        /// <param name="text">The JSON string to deserialize. Must not be <see langword="null"/> or empty.</param>
        /// <returns>
        /// The deserialized <see cref="Message"/> instance, or <see langword="null"/> if deserialization fails.
        /// </returns>
        public static Message? Deserialize(string text)
        {
            return JsonSerializer.Deserialize<Message?>(text, _options);
        }
    }
}