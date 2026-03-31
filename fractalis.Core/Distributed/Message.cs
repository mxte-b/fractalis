using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace fractalis.Core.Distributed
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(RegistrationMessage), "registration")]
    [JsonDerivedType(typeof(ReconnectMessage), "reconnect")]
    [JsonDerivedType(typeof(RegistrationAcknowledgedMessage), "registrationAcknowledged")]
    [JsonDerivedType(typeof(DebugMessage), "debug")]
    public abstract record Message;

    public record RegistrationMessage : Message
    {
        [JsonPropertyName("displayName")]
        public required string          DisplayName { get; init; }
    }

    public record ReconnectMessage : Message
    {
        [JsonPropertyName("clientId")]
        public required Guid            ClientId    { get; init; }
    }

    public record RegistrationAcknowledgedMessage : Message
    {
        [JsonPropertyName("clientId")]
        public required Guid            ClientId    { get; init; }
    }

    public record DebugMessage : Message
    {
        [JsonPropertyName("content")]
        public required string          Content     { get; init; }
    }

    public static class MessageSerializer
    {
        public static byte[] Serialize(Message message)
        {
            string text = JsonSerializer.Serialize(message);
            return Encoding.UTF8.GetBytes(text);
        }
    }
}
