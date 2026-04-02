using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using System.Text.Json.Serialization;

namespace fractalis.Core
{
    /// <summary>
    /// Predefined palette presets available for quick access.
    /// </summary>
    public enum PalettePreset
    {
        BB,
        Midnight,
        RedAccent,
        Glacial,
        PurpleFlame,
    }

    /// <summary>
    /// Represents a named color palette with a list of color stops.
    /// </summary>
    public record PaletteData
    {
        /// <summary>
        /// The name of the palette.
        /// </summary>
        [JsonPropertyName("name")]
        public required string          Name        { get; init; }

        /// <summary>
        /// The list of color stops defining the palette gradient.
        /// </summary>
        [JsonPropertyName("stops")]
        public required List<ColorStop> ColorStops  { get; init; }
    }

    /// <summary>
    /// Represents a single point in a color gradient.
    /// </summary>
    /// <param name="position">The normalized position (0–1) of the stop in the gradient.</param>
    /// <param name="color">The color at this stop.</param>
    public struct ColorStop(float position, Color color)
    {
        /// <summary>
        /// Normalized position of the stop in the gradient (0 = start, 1 = end).
        /// </summary>
        [JsonPropertyName("position")]
        public float    Position { get; set; } = position;

        /// <summary>
        /// Color at this stop, serialized as a Vector4 for JSON compatibility.
        /// </summary>
        [JsonPropertyName("color")]
        [JsonConverter(typeof(Vector4Converter))]
        public Vector4  Color    { get; set; } = color.ToPixel<Rgba32>().ToVector4();
    }

    /// <summary>
    /// Manages a continuous color gradient with a lookup table for efficient sampling.
    /// </summary>
    public class ColorPalette
    {
        /// <summary>
        /// The number of iterations over which the palette repeats.
        /// </summary>
        public int                          Frequency       { get; set; }

        /// <summary>
        /// Offset applied to the palette sampling for shifting the gradient.
        /// </summary>
        public float                        Offset          { get; set; }

        /// <summary>
        /// Color used for points inside the fractal.
        /// </summary>
        public Color                        InteriorColor   { get; set; }

        /// <summary>
        /// Resolution of the internal lookup table (LUT) used for fast gradient sampling.
        /// </summary>
        public static int                   LutResolution   { get; set; } = 4096;

        private readonly List<ColorStop>    _stops;
        private readonly Vector4[]          _lut;
        private static readonly ResourceManager    _resourceManager = ResourceManager.Instance;

        /// <summary>
        /// Creates an empty palette.
        /// </summary>
        public ColorPalette()
        {
            _stops = new List<ColorStop>();
            _lut = new Vector4[LutResolution];
        }

        /// <summary>
        /// Creates a palette initialized with the specified color stops.
        /// </summary>
        /// <param name="stops">Color stops defining the gradient.</param>
        public ColorPalette(IEnumerable<ColorStop> stops)
        {
            _stops = stops.ToList();
            _lut = new Vector4[LutResolution];
            BakeLUT();
        }

        /// <summary>
        /// Creates a palette from a predefined <see cref="PalettePreset"/>.
        /// </summary>
        /// <param name="preset">The preset to load.</param>
        /// <returns>A <see cref="ColorPalette"/> instance with preset stops.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the preset is not found in <see cref="ResourceManager"/>.</exception>
        public static ColorPalette FromPreset(PalettePreset preset)
        {
            List<ColorStop>? stops;
            _resourceManager.ColorPalettes.TryGetValue(preset.ToString(), out stops);

            if (stops == null)
            {
                throw new KeyNotFoundException($"The key '{preset}' is not a valid color palette.");
            }

            return new ColorPalette(stops);
        }

        /// <summary>
        /// Adds a color stop to the palette and rebuilds the LUT.
        /// </summary>
        /// <param name="stop">The color stop to add.</param>
        public void AddStop(ColorStop stop)
        {
            _stops.Add(stop);
            BakeLUT();
        }

        /// <summary>
        /// Removes a color stop by index and rebuilds the LUT.
        /// </summary>
        /// <param name="index">Index of the stop to remove.</param>
        public void RemoveStop(int index)
        {
            _stops.RemoveAt(index);
            BakeLUT();
        }

        /// <summary>
        /// Clears all color stops and rebuilds the LUT.
        /// </summary>
        public void ClearStops()
        {
            _stops.Clear();
            BakeLUT();
        }

        /// <summary>
        /// Rebuilds the lookup table from the current list of color stops.
        /// </summary>
        private void BakeLUT()
        {
            if (_stops.Count == 0) return;
            if (_stops.Count == 1)
            {
                Vector4 stop = _stops[0].Color;
                for (int i = 0; i < LutResolution; i++)
                {
                    _lut[i] = stop;
                }

                return;
            }

            for (int i = 0; i < LutResolution; i++)
            {
                _lut[i] = SampleStops(i / (float)(LutResolution - 1));
            }
        }

        /// <summary>
        /// Interpolates between color stops to get the color at a normalized position.
        /// </summary>
        /// <param name="t">Normalized position in [0,1].</param>
        /// <returns>Interpolated color as a <see cref="Vector4"/>.</returns>
        private Vector4 SampleStops(float t)
        {
            // Normalizing the iteration with repeating
            if (_stops.Count == 1) return _stops[0].Color;

            // Selecting the stops that bracket the value
            ColorStop left = _stops.LastOrDefault(s => s.Position <= t);
            ColorStop right = _stops.FirstOrDefault(s => s.Position >= t);

            if (left.Position == right.Position) return left.Color;

            // Interpolating between them
            float localT = (t - left.Position) / (right.Position - left.Position);
            return Vector4.Lerp(left.Color, right.Color, localT);
        }

        /// <summary>
        /// Samples the palette at a specific smooth iteration value.
        /// </summary>
        /// <param name="smoothIteration">The iteration count or fractional iteration for fractal coloring.</param>
        /// <returns>The corresponding color as a <see cref="Vector4"/>.</returns>
        public Vector4 Sample(double smoothIteration)
        {
            if (_lut.Length == 0) return Vector4.Zero;

            double normalized = (smoothIteration % Frequency) / Frequency;
            double shifted = (normalized + Offset) % 1.0;
            if (shifted < 0) shifted += 1.0;

            int index = (int)(shifted * (LutResolution - 1));
            return _lut[index];
        }
    }
}