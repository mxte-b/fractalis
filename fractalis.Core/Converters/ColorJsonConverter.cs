using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace fractalis.Core.Converters
{
    public class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Vector4JsonConverter conv = new();
            Vector4 vec = conv.Read(ref reader, typeToConvert, options);

            return Color.FromRgba(
                (byte)vec.X,
                (byte)vec.Y,
                (byte)vec.Z,
                (byte)vec.W
            );
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            Vector4 col = value.ToPixel<Rgba32>().ToVector4();
            col *= 255;

            Vector4JsonConverter conv = new();
            conv.Write(writer, col, options);
        }
    }
}
