using fractalis.Core.Numbers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace fractalis.Core.Converters
{
    public class BigFloatJsonConverter : JsonConverter<BigFloat>
    {
        public override BigFloat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? value = reader.GetString();
            return value is null ? throw new JsonException("Expected a string for BigFloat.") : new BigFloat(value);
        }

        public override void Write(Utf8JsonWriter writer, BigFloat value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToFullString());
        }
    }
}
