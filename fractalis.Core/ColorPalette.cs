using Microsoft.VisualBasic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace fractalis.Core
{
    public enum PalettePreset { BB, Midnight, RedAccent }

    public record PaletteData
    {
        [JsonPropertyName("name")]
        public required string          Name        { get; init; }

        [JsonPropertyName("stops")]
        public required List<ColorStop> ColorStops  { get; init; }
    }

    public struct ColorStop(float position, Color color)
    {
        [JsonPropertyName("position")]
        public float    Position { get; set; } = position;

        [JsonPropertyName("color")]
        [JsonConverter(typeof(Vector4Converter))]
        public Vector4  Color    { get; set; } = color.ToPixel<Rgba32>().ToVector4();
    }

    public class ColorPalette
    {
        public int                          Frequency       { get; set; }
        public float                        Offset          { get; set; }
        public Color                        InteriorColor   { get; set; }
        public static int                   LutResolution   { get; set; } = 4096;

        private readonly List<ColorStop>    _stops;
        private readonly Vector4[]          _lut;
        private static readonly ResourceManager    _resourceManager = ResourceManager.Instance;

        public ColorPalette()
        {
            _stops = new List<ColorStop>();
            _lut = new Vector4[LutResolution];
        }

        public ColorPalette(IEnumerable<ColorStop> stops)
        {
            _stops = stops.ToList();
            _lut = new Vector4[LutResolution];
            BakeLUT();
        }

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

        public void AddStop(ColorStop stop) 
        {
            _stops.Add(stop);
            BakeLUT();
        }

        public void RemoveStop(int index)
        {
            _stops.RemoveAt(index);
            BakeLUT();
        }

        public void ClearStops()
        {
            _stops.Clear();
            BakeLUT();
        }

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
