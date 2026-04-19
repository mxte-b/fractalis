using fractalis.Core.Distributed.Clients;
using fractalis.Core.Distributed.Networking.Messages;
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
    [JsonDerivedType(typeof(RenderAssignmentStatusMessage), "assignmentStatus")]
    [JsonDerivedType(typeof(RegistrationAcknowledgedMessage), "registrationAcknowledged")]
    public abstract record Message;

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