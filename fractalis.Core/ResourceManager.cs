using System.Reflection;
using System.Text.Json;

namespace fractalis.Core
{
    public class ResourceManager
    {
        private static ResourceManager _instance = new();
        private static readonly object _lock = new();

        public Dictionary<string, List<ColorStop>> ColorPalettes = [];
        public Dictionary<string, Sight> Sights = [];

        private ResourceManager()
        {
            ColorPalettes = LoadColorPalettes();
            Sights = LoadSights();
        }

        private static string ReadEmbeddedResource(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream? stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException($"Couldn't find embedded resource '{resourceName}'.");

            StreamReader reader = new(stream);
            return reader.ReadToEnd();
        }

        private static Dictionary<string, List<ColorStop>> LoadColorPalettes()
        {
            string text = ReadEmbeddedResource("fractalis.Core.Resources.palettes.json");

            return JsonSerializer.Deserialize<Dictionary<string, List<ColorStop>>>(text, FractalisJsonOptions.Default) 
                ?? throw new FormatException("The palette data was malformed.");
        }

        private static Dictionary<string, Sight> LoadSights()
        {
            string text = ReadEmbeddedResource("fractalis.Core.Resources.sights.json");

            return JsonSerializer.Deserialize<Dictionary<string, Sight>>(text, FractalisJsonOptions.Default)
                ?? throw new FormatException("The sights data was malformed.");
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
