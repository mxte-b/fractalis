using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Diagnostics;
using fractalis.Core;
using fractalis.Core.Fractals;
using fractalis.Core.Numbers;
using fractalis.Core.Video;

namespace fractalis
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Banner.V1);

            int w = 400;
            int h = 400;

            BigComplex center = Sights.RetroDays;

            BigFloat zoom = new BigFloat("1e300");

            int iterations = 2000;

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

            FractalRendererConfig rendererConfig = new FractalRendererConfig()
            {
                Fractal = new Mandelbrot(),
                Iterations = iterations,
                Width = w,
                Height = h,
                Zoom = zoom,
                Center = center,
                ColorPalette = palette,
            };

            FractalRenderer renderer = new FractalRenderer(rendererConfig);

            //Image<Rgb24> image = renderer.Render(true);
            //image.Save("render.png");

            //FractalBenchmark bench = new FractalBenchmark(rendererConfig);
            //bench.Run("My custom AVX", 10);

            VideoConfig config = new VideoConfig()
            {
                Duration = 1 * 60,
                FPS = 30,
                ZoomStart = new BigFloat("0.5"),
                ZoomEnd = new BigFloat("1e15"),
                StartAnimationDuration = 1,
                StopAnimationDuration = 2,
            };
            VideoRenderer videoRenderer = new VideoRenderer(renderer, config);

            videoRenderer.Start();
            videoRenderer.Save();

            //Process.Start(new ProcessStartInfo("render.png") { UseShellExecute = true });
        }
    }
}
