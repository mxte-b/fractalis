using System.Reflection;
using System.Text.Json;

namespace fractalis.Core
{
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

            var data = JsonSerializer.Deserialize<Dictionary<string, List<ColorStop>>>(text, FractalisJsonOptions.Default) 
                ?? throw new FormatException("The palette data was malformed.");

            ColorPalettes = data;
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
