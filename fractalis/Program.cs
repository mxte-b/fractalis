using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Diagnostics;
using fractalis.Core;
using fractalis.Core.Fractals;

namespace fractalis
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Banner.V1);
            int w = 3840;
            int h = 2160;

            BigComplex center = new(-.5,0);

            BigFixed zoom = new BigFixed("0.5");

            int iterations = 100;

            ColorPalette palette = new ColorPalette();
            palette.InteriorColor = Color.Black;
            palette.MaxIterations = iterations;
            palette.Frequency = 200;

            palette.AddStop(new(0f, Color.FromRgb(0, 7, 100)));
            palette.AddStop(new(0.2f, Color.FromRgb(32, 107, 203)));
            palette.AddStop(new(0.4f, Color.FromRgb(237, 255, 255)));
            palette.AddStop(new(0.6f, Color.FromRgb(255, 170, 0)));
            palette.AddStop(new(0.8f, Color.FromRgb(0, 2, 0)));
            palette.AddStop(new(1f, Color.FromRgb(0, 7, 100)));

            var renderer = new FractalRenderer<Mandelbrot>(iterations, w, h, zoom, center);
            renderer.ColorPalette = palette;

            Image<Rgb24> image = renderer.Render();
            image.Save("render.png");

            Process.Start(new ProcessStartInfo("render.png") { UseShellExecute = true });
        }
    }
}
