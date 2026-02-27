using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Diagnostics;
using fractalis.Core;

namespace fractalis
{
    internal class Program
    {
        static void Main(string[] args)
        {

            int w = 800;
            int h = 600;

            BigComplex center = Sights.HardestTrip;

            BigFixed zoom = new BigFixed("1e150");

            int iterations = 100000;

            ColorPalette palette = new ColorPalette();
            palette.InteriorColor = Color.Black;
            palette.MaxIterations = iterations;
            palette.Frequency = 200;

            palette.AddStop(new(0, Color.Navy));
            palette.AddStop(new(0.3f, Color.DarkSlateBlue));
            palette.AddStop(new(0.6f, Color.PapayaWhip));
            palette.AddStop(new(0.8f, Color.PaleVioletRed));
            palette.AddStop(new(1, Color.Navy));

            MandelbrotRenderer renderer = new MandelbrotRenderer(iterations, w, h, zoom, center);
            MandelbrotRenderer.ColorPalette = palette;

            Image<Rgb24> image = renderer.Render();
            image.Save("render.png");

            Process.Start(new ProcessStartInfo("render.png") { UseShellExecute = true });
        }
    }
}
