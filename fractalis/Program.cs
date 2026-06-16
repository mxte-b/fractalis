using fractalis.Core;
using fractalis.Core.Miscellaneous;
using fractalis.Core.Video;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Spectre.Console;
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

            AppSettings settings = AppConfigurator.Configure(args);
            FractalRenderer renderer = new(settings.FractalRendererConfig);

            switch (settings.Mode)
            {
                case AppMode.Image:
                    Image<Rgba32> image = renderer.Render();
                    image.Save(settings.OutputPath);

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

                            distributed.Save(settings.OutputPath);
                            break;

                        // Local video rendering using local compute
                        case VideoMode.Local:
                            VideoRenderer local = new(renderer, settings.VideoConfig);
                            local.Start();
                            local.Save(settings.OutputPath);
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

            // Prevent app from closing instantly after it finishes
            if (!Console.IsInputRedirected)
            {
                AnsiConsole.MarkupLine($"[{ThemeColor.Muted}]Press any key to exit...[/]");
                Console.ReadKey(true);
            }
        }
    }
}
