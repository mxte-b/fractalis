using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace fractalis.Core.Converters
{
    public class Vector4JsonConverter : JsonConverter<Vector4>
    {
        public override Vector4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            float[] values = JsonSerializer.Deserialize<float[]>(ref reader, options)!;
            return new Vector4(values[0], values[1], values[2], values[3]);
        }

        public override void Write(Utf8JsonWriter writer, Vector4 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteNumberValue(value.Z);
            writer.WriteNumberValue(value.W);
            writer.WriteEndArray();
        }
    }
}
