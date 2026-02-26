using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace fractalis.Components
{
    public enum PalettePreset { Grayscale, Fire, Ocean, Electric, Rainbow }

    public class ColorPalette
    {
        // --- Inner type ---
        public struct ColorStop
        {
            public float Position { get; set; }
            public Vector4 Color { get; set; }
            public ColorStop(float position, Color color) 
            {
                Position = position;
                Color = color.ToPixel<Rgba32>().ToVector4();
            }
        }

        // --- Properties ---
        public int MaxIterations { get; set; }
        public int Frequency { get; set; }
        public float Offset { get; set; }
        public Color InteriorColor { get; set; }
        private List<ColorStop> Stops { get; }

        // --- Constructors ---
        public ColorPalette() => Stops = new List<ColorStop>();

        public ColorPalette(IEnumerable<ColorStop> stops) => Stops = stops.ToList();

        // --- Presets / Factory ---
        public static ColorPalette FromPreset(PalettePreset preset) => throw new NotImplementedException();

        // --- Stop management ---
        public void AddStop(ColorStop stop) => Stops.Add(stop);
        public void RemoveStop(int index) => Stops.RemoveAt(index);
        public void ClearStops() => Stops.Clear();

        // --- Sampling ---
        public Color Sample(double iteration)
        {

            if (iteration == MaxIterations) return InteriorColor;

            // Normalizing the iteration with repeating
            double idx = iteration % Frequency;
            float normalized = (float)idx / Frequency;

            //Console.WriteLine(normalized);

            // Selecting the stops that bracket the value
            ColorStop left = Stops.LastOrDefault(x => x.Position <= normalized);
            ColorStop right = Stops.FirstOrDefault(x => x.Position >= normalized);

            if (left.Position == right.Position) return new Color(left.Color);

            // Interpolating between them
            float t = (normalized - left.Position) / (right.Position - left.Position);
            Vector4 interpolated = Vector4.Lerp(left.Color, right.Color, t);

            return new Color(interpolated);
        }

        // --- Serialization ---
        public string ToJson() => throw new NotImplementedException();
        public static ColorPalette FromJson(string json) => throw new NotImplementedException();
        public ColorPalette Clone() => throw new NotImplementedException();
    }
}
