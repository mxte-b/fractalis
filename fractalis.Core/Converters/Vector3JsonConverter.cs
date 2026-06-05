using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace fractalis.Core.Converters
{
    public class Vector3JsonConverter : JsonConverter<Vector3>
    {
        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            float[] values = JsonSerializer.Deserialize<float[]>(ref reader, options)!;
            return new Vector3(values[0], values[1], values[2]);
        }

        public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteNumberValue(value.Z);
            writer.WriteEndArray();
        }
    }
}
