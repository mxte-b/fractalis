using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fractalis.Core
{
    public class Vector4Converter : JsonConverter<Vector4>
    {
        public override Vector4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            float[] values = JsonSerializer.Deserialize<float[]>(ref reader, options)!;
            return new Vector4(values[0] / 255f, values[1] / 255f, values[2] / 255f, values[3] / 255f);
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

    public class ResourceManager
    {
        private static ResourceManager _instance = new ResourceManager();
        private static readonly object _lock = new object();

        public Dictionary<string, List<ColorStop>> ColorPalettes = [];

        private ResourceManager()
        {
            LoadColorPalettes();
        }

        private void LoadColorPalettes()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream? stream = assembly.GetManifestResourceStream("fractalis.Core.Resources.palettes.json");

            if (stream == null)
            {
                throw new FileNotFoundException("Couldn't find color palette embedded resource.");
            }

            StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new Vector4Converter());
            List<PaletteData>? data = JsonSerializer.Deserialize<List<PaletteData>>(text, options);
            if (data == null)
            {
                throw new FormatException("The palette data was malformed.");
            }

            foreach (PaletteData palette in data)
            {
                ColorPalettes.Add(palette.Name, palette.ColorStops);
            }
        }

        public static ResourceManager Instance
        {
            get
            {
                lock (_lock)
                {
                    _instance ??= new ResourceManager();

                    return _instance;
                }
            }
        }
    }
}
