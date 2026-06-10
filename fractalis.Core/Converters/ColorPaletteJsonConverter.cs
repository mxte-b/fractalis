using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fractalis.Core.Converters
{
    internal record ColorPaletteDto
    {
        public string? Preset { get; init; }
        public int? Frequency { get; init; }
        public float? Offset { get; init; }

        [JsonConverter(typeof(ColorJsonConverter))]
        public Color? InteriorColor { get; init; }
        public List<ColorStop>? Stops { get; init; }
    }

    internal class ColorPaletteJsonConverter : JsonConverter<ColorPalette>
    {
        public override ColorPalette Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dto = JsonSerializer.Deserialize<ColorPaletteDto>(ref reader, FractalisJsonOptions.Default) 
                ?? throw new JsonException("Invalid color palette JSON.");

            if (dto.Preset is null && dto.Stops is null) 
                throw new JsonException("ColorPalette must specify either 'preset' or 'stops'.");

            ColorPalette palette;

            if (dto.Preset is not null)
            {
                palette = ColorPalette.FromPreset(Enum.Parse<PalettePreset>(dto.Preset));
            }
            else
            {
                palette = new ColorPalette(dto.Stops!);
            }

            // Overriding palette data
            if (dto.Frequency.HasValue)     palette.Frequency     = dto.Frequency.Value;
            if (dto.Offset.HasValue)        palette.Offset        = dto.Offset.Value;
            if (dto.InteriorColor.HasValue) palette.InteriorColor = dto.InteriorColor.Value;

            return palette;
        }

        public override void Write(Utf8JsonWriter writer, ColorPalette value, JsonSerializerOptions options)
        {
            var dto = new ColorPaletteDto()
            {
                Preset          = value.PresetName,
                Frequency       = value.Frequency != ColorPalette.DefaultFrequency ? value.Frequency : null,
                Offset          = value.Offset != ColorPalette.DefaultOffset ? value.Offset : null,
                InteriorColor   = value.InteriorColor != ColorPalette.DefaultInteriorColor ? value.InteriorColor : null,
                Stops           = value.PresetName != null ? null : value.Stops
            };

            JsonSerializer.Serialize(writer, dto, FractalisJsonOptions.Default);
        }
    }
}
