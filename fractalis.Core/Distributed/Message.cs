using System.Text.Json.Serialization;

namespace fractalis.Core.Distributed
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(RegistrationMessage), "registration")]
    [JsonDerivedType(typeof(ReconnectMessage), "reconnect")]
    [JsonDerivedType(typeof(RegistrationAcknowledgedMessage), "registrationAcknowledged")]
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
}
