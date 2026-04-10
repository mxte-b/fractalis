//#define BENCHMARK
using fractalis.Core;
using fractalis.Core.Fractals;
using fractalis.Core.Numbers;
using fractalis.Core.Video;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Text.Json;

namespace fractalis
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(Banner.V1);
#if BENCHMARK
            int w = 400;
            int h = 400;

            BigComplex center = Sights.Test;
            BigFloat zoom = new BigFloat("1e300");
            int iterations = 80000;
#else
            int w = 1920;
            int h = 1080;

            BigComplex center = Sights.MagnumOpusEx;
            BigFloat zoom = new BigFloat("1e170");
            int iterations = 16000;
#endif

            ColorPalette palette = ColorPalette.FromPreset(PalettePreset.PurpleFlame);
            palette.InteriorColor = Color.Black;
            palette.Frequency = 200;
            palette.Offset = 0.4f;

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
            //Image<Rgb24> image = renderer.Render(true);
            //image.Save("render.png");
            //Process.Start(new ProcessStartInfo("render.png") { UseShellExecute = true });

            VideoConfig config = new VideoConfig()
            {
                Duration = 1 * 60,
                FPS = 30,
                ZoomStart = new BigFloat("0.5"),
                ZoomEnd = new BigFloat("1e15"),
                StartAnimation = new AnimationSettings()
                {
                    Duration = 3,
                    Exponent = 2.5
                },
                StopAnimation = new AnimationSettings()
                {
                    Duration = 3
                },

                // For the ability to split the work into multiple sessions
                //StartFrame = 17393,
                //RenderIdOverride = "a9f5523e-bd93-4cd1-ac70-19d59a0e5018"
            };

            //Console.WriteLine(JsonSerializer.Serialize(rendererConfig));
            DistributedVideoRenderer videoRenderer = new DistributedVideoRenderer(renderer, config);
            videoRenderer.Initialize();

            await videoRenderer.Start(new Uri("ws://localhost:5059/ws"), rendererConfig);
            //videoRenderer.Save();
#endif
        }
    }
}
