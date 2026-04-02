using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace fractalis.Core.Distributed
{
    /// <summary>
    /// Base class for all message types exchanged between clients and the orchestrator.
    /// </summary>
    /// <remarks>
    /// Uses JSON polymorphic serialization with a type discriminator property named "type".
    /// </remarks>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(RegistrationMessage), "registration")]
    [JsonDerivedType(typeof(ReconnectMessage), "reconnect")]
    [JsonDerivedType(typeof(RegistrationAcknowledgedMessage), "registrationAcknowledged")]
    [JsonDerivedType(typeof(DebugMessage), "debug")]
    public abstract record Message;

    /// <summary>
    /// Message sent by a client to register itself with the orchestrator.
    /// </summary>
    public record RegistrationMessage : Message
    {
        /// <summary>
        /// The display name of the client for identification purposes.
        /// </summary>
        [JsonPropertyName("displayName")]
        public required string          DisplayName { get; init; }
    }

    /// <summary>
    /// Message sent by a client to reconnect using its existing client ID.
    /// </summary>
    public record ReconnectMessage : Message
    {
        /// <summary>
        /// The unique identifier of the client attempting to reconnect.
        /// </summary>
        [JsonPropertyName("clientId")]
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
        [JsonPropertyName("clientId")]
        public required Guid            ClientId    { get; init; }
    }

    /// <summary>
    /// Message used for debugging purposes, containing arbitrary text content.
    /// </summary>
    public record DebugMessage : Message
    {
        /// <summary>
        /// The debug message content.
        /// </summary>
        [JsonPropertyName("content")]
        public required string          Content     { get; init; }
    }

    /// <summary>
    /// Provides serialization utilities for <see cref="Message"/> objects.
    /// </summary>
    public static class MessageSerializer
    {
        /// <summary>
        /// Serializes a <see cref="Message"/> into a UTF-8 encoded byte array.
        /// </summary>
        /// <param name="message">The message to serialize. Must not be <see langword="null"/>.</param>
        /// <returns>A UTF-8 encoded byte array representing the JSON-serialized message.</returns>
        public static byte[] Serialize(Message message)
        {
            string text = JsonSerializer.Serialize(message);
            return Encoding.UTF8.GetBytes(text);
        }
    }
}