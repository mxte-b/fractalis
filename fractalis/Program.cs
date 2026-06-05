using fractalis.Core;
using fractalis.Core.Compositor;
using fractalis.Core.Compositor.Layers.Stylistic;
using fractalis.Core.Miscellaneous;
using fractalis.Core.Video;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Text;

namespace fractalis
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine(Banner.V1);
            Console.WriteLine(Watermarks.FractalisBlack);

            LayerCompositor compositor = new LayerCompositor()
                //.AddLayer(new ASCIIArtLayer(0.5f))
                .AddLayer(new ChromaticAberrationLayer(new(0.006f, 0.003f, -0.003f)))
                .AddLayer(new VignetteLayer(1, 1.5f))
                .AddLayer(new WatermarkLayer(
                    Watermarks.FractalisWhite,
                    new WatermarkOptions()
                    {
                        Scale = 0.5f,
                        Opacity = 0.4f
                    }
                ));

            AppSettings settings = AppConfigurator.Configure(args);
            FractalRenderer renderer = new(settings.FractalRendererConfig with { LayerCompositor = compositor});

            switch (settings.Mode)
            {
                case AppMode.Image:
                    Image<Rgba32> image = renderer.Render();
                    image.Save("render.png");

                    if (settings.OpenRenderedImage)
                    {
                        Process.Start(new ProcessStartInfo("render.png") { UseShellExecute = true });
                    }
                    break;

                case AppMode.Video:
                    if (settings.VideoConfig is null) throw new Exception("VideoConfig is null.");

                    switch (settings.VideoMode)
                    {
                        // Distributed video rendering using network devices
                        case VideoMode.Distributed:
                            var distributedSettings = settings.DistributedRendererSettings
                                ?? throw new Exception("DistributedRendererConfig is null.");

                            DistributedVideoRenderer distributed = new(settings.FractalRendererConfig, settings.VideoConfig);
                            await distributed.Start(distributedSettings.OrchestratorUri, distributedSettings.FrameListenerPort);

                            distributed.Save();
                            break;

                        // Local video rendering using local compute
                        case VideoMode.Local:
                            VideoRenderer local = new(renderer, settings.VideoConfig);
                            local.Start();
                            local.Save();
                            break;

                        default: throw new Exception($"Unknown video mode encountered: {settings.VideoConfig}");
                    }

                    break;

                case AppMode.Benchmark:
                    if (settings.FractalBenchmarkConfig is null) throw new Exception("FractalBenchmarkConfig is null.");

                    FractalBenchmark benchmark = new(settings.FractalRendererConfig);
                    benchmark.Run(settings.FractalBenchmarkConfig.Label, settings.FractalBenchmarkConfig.Runs);
                    break;
            }
        }
    }
}
