using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed
{
    public enum MessageType
    {
        Registration,
        Reconnect,
        RegistrationAcknowledged
    }

    [JsonConverter(typeof(MessageConverter))]
    public abstract record Message
    {
        [JsonPropertyName("type")]
        public abstract MessageType     Type        { get; }
    }

    public record RegistrationMessage : Message
    {
        public override MessageType     Type        => MessageType.Registration;
        [JsonPropertyName("displayName")]
        public required string          DisplayName { get; init; }
    }

    public record ReconnectMessage : Message 
    {
        public override MessageType     Type        => MessageType.Reconnect;
    }

    public record RegistrationAcknowledgedMessage : Message
    {
        public override MessageType     Type        => MessageType.RegistrationAcknowledged;
        [JsonPropertyName("clientId")]
        public required Guid            ClientId    { get; init; }
    }

    public class MessageConverter : JsonConverter<Message>
    {
        public override Message? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            var root = document.RootElement;

            // Access the type property
            bool hasType = root.TryGetProperty("type", out var typeJson);

            if (!hasType) throw new JsonException("Missing 'type' property");

            string? type = typeJson.GetString();
            return type switch
            {
                "Registration" => JsonSerializer.Deserialize<RegistrationMessage>(root.GetRawText(), options),
                "Reconnect" => JsonSerializer.Deserialize<ReconnectMessage>(root.GetRawText(), options),
                "RegistrationAcknowledged" => JsonSerializer.Deserialize<RegistrationAcknowledgedMessage>(root.GetRawText(), options),
                _ => throw new JsonException($"Unknown message type: {type}")
            };
        }

        public override void Write(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("type", value.Type.ToString());

            foreach (var prop in value.GetType().GetProperties())
            {
                if (prop.Name == nameof(value.Type)) continue;

                var propValue = prop.GetValue(value);
                var jsonName = char.ToLower(prop.Name[0]) + prop.Name[1..];

                writer.WritePropertyName(jsonName);
                JsonSerializer.Serialize(writer, propValue, options);
            }

            writer.WriteEndObject();
        }
    }
}
