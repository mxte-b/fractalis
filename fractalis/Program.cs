//#define BENCHMARK
using fractalis.Core;
using fractalis.Core.Fractals;
using fractalis.Core.Numbers;
using fractalis.Core.Video;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;

namespace fractalis
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Banner.V1);
#if BENCHMARK
            int w = 400;
            int h = 400;

            BigComplex center = Sights.Test;
            BigFloat zoom = new BigFloat("1e300");
            int iterations = 80000;
#else
            int w = 400;
            int h = 400;

            BigComplex center = Sights.Test;
            BigFloat zoom = new BigFloat("1e500");
            int iterations = 80000;
#endif

            ColorPalette palette = ColorPalette.FromPreset(PalettePreset.Midnight);
            palette.InteriorColor = Color.Black;
            palette.Frequency = 1500;

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
#if BENCHMARK
            FractalBenchmark bench = new FractalBenchmark(rendererConfig);
            bench.Run("Baseline", 10);
#else
            Image<Rgb24> image = renderer.Render(true);
            image.Save("render.png");
            Process.Start(new ProcessStartInfo("render.png") { UseShellExecute = true });

            //VideoConfig config = new VideoConfig()
            //{
            //    Duration = 11 * 60,
            //    FPS = 30,
            //    ZoomStart = new BigFloat("0.5"),
            //    ZoomEnd = new BigFloat("1e200"),
            //    StartAnimationDuration = 0.5,
            //    StopAnimationDuration = 3,
            //};
            //VideoRenderer videoRenderer = new VideoRenderer(renderer, config);

            //videoRenderer.Start();
            //videoRenderer.Save();
#endif
        }
    }
}
