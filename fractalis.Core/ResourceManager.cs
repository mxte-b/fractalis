using System.Reflection;
using System.Text.Json;

namespace fractalis.Core
{
    public class ResourceManager
    {
        private static ResourceManager _instance = new ResourceManager();
        private static readonly object _lock = new object();
        private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

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

            List<PaletteData>? data = JsonSerializer.Deserialize<List<PaletteData>>(text, _serializerOptions);
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
