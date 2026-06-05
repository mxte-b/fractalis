using fractalis.Core.Compositor;
using fractalis.Core.Compositor.Layers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace fractalis.Core.Converters
{
    public class LayerCompositorConverter : JsonConverter<LayerCompositor>
    {
        public override void Write(Utf8JsonWriter writer, LayerCompositor value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("layers");
            JsonSerializer.Serialize(writer, value._layers, options);
            writer.WriteEndObject();
        }

        public override LayerCompositor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var compositor = new LayerCompositor();

            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            if (root.TryGetProperty("layers", out var layersElement))
            {
                compositor._layers = JsonSerializer.Deserialize<List<CompositeLayer>>(layersElement.GetRawText(), options) ?? [];
            }

            return compositor;
        }
    }
}
