using fractalis.Core.Fractals;
using fractalis.Core.Miscellaneous.Phases;
using fractalis.Core.Compositor;
using fractalis.Core.Renderers;
using Spectre.Console;
using System.Text.Json;
using SixLabors.ImageSharp;
using fractalis.Core.Video;

namespace fractalis.Core.Miscellaneous
{
    public static class AppConfigurator
    {
        internal static readonly IEnumerable<string> ImageFormats = Configuration.Default.ImageFormats.SelectMany(f => f.FileExtensions).Select(e => "." + e);
        internal static readonly IEnumerable<string> VideoFormats = [".mp4", ".avi", ".mkv", ".mov", ".webm"];
        private static AppMode PromptAppMode() 
            => Prompts.Selection($"What would you like to [bold {ThemeColor.Accent}]do[/]?", 
                ["Image rendering", "Video rendering", "Benchmarking", "Video recovery"])
            .Convert(choice => choice switch
            {
                "Image rendering" => AppMode.Image,
                "Video rendering" => AppMode.Video,
                "Benchmarking" => AppMode.Benchmark,
                "Video recovery" => AppMode.VideoRecovery,
                _ => AppMode.Image
            });

        private static string PromptOutputPath(AppMode mode)
        {
            var defaultValue = mode switch
            {
                AppMode.Image => "render.png",
                AppMode.Video => "render.mp4",
                AppMode.Benchmark => "benchmark.json",
                _ => throw new Exception($"Unknown AppMode: {mode}")
            };

            return Prompts.SavePath(
                $"[{ThemeColor.Accent}]Where[/] should the output be saved to?",
                defaultValue: defaultValue,
                allowedFormats: mode switch
                {
                    AppMode.Image => ImageFormats,
                    AppMode.Video => VideoFormats,
                    AppMode.Benchmark => [".json"],
                    _ => null
                }
            );
        }

        private static AppSettings? ParseConfig(string path) => JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path), FractalisJsonOptions.Default);

        private static AppSettings? TryLoadConfig(string[] args)
        {
            // Try arguments first
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--config" && i + 1 < args.Length)
                {
                    string path = args[i + 1];

                    Prompts.Info($"Trying to load config from path {path}");
                    return ParseConfig(path);
                }
            }

            // Then ask for path to config file
            var isManualConfig = Prompts.Selection(
                $"[{ThemeColor.Accent}]How[/] do you want to configure the app?",
                ["Configure manually", "Load configuration from file"]
            ).Convert(choice => choice == "Configure manually");

            if (!isManualConfig)
            {
                var configPath = Prompts.FilePath(
                    $"What is the [{ThemeColor.Accent}]path[/] of the config file?",
                    allowedFormats: [".json"]
                    );

                var config = ParseConfig(configPath);

                if (config is not null) return config;
            }

            return null;
        }

        private static AppSettings? TryRecoverVideo()
        {
            Prompts.Section("Recovery");
            var recoveryPath = Prompts.FilePath(
                title: $"[{ThemeColor.Accent}]Path[/] of the video recovery file?",
                hint: "[grey]Recovery files are saved in the same folder as the frames of the video.[/]",
                allowedFormats: [".json"]
                );

            try
            {
                return VideoRecovery.Recover(recoveryPath);
            }
            catch (Exception e)
            {
                Prompts.Warn($"An error occured while recovering the video ({e.Message}). Proceeding with normal configuration.");
            }

            Prompts.Done();
            return null;
        }

        public static AppSettings Configure(string[] args)
        {
            AppSettings? config = TryLoadConfig(args);
            if (config is not null)
            {
                Prompts.Success("Configuration successfully loaded.");
                return config;
            }

            AnsiConsole.MarkupLine($"[bold {ThemeColor.Title}]Welcome to the Fractalis Configurator![/]");
            AnsiConsole.WriteLine();

            var mode = PromptAppMode();
            if (mode == AppMode.VideoRecovery)
            {
                // Only return if the video was successfully recovered
                if (TryRecoverVideo() is AppSettings t) return t;
            }

            var video = mode == AppMode.Video ? ConfigureVideo() : null;
            var rendererConfig = ConfigureRenderer(mode, video?.VideoMode);
            var benchmarkConfig = mode == AppMode.Benchmark ? ConfigureBenchmark() : null;

            Prompts.Section("Export");
            var outputPath = mode != AppMode.Benchmark ? PromptOutputPath(mode) : null;

            AppSettings settings = new()
            {
                Mode = mode,
                FractalRendererConfig = rendererConfig,
                OutputPath = outputPath,
                VideoMode = video?.VideoMode,
                VideoConfig = video?.VideoConfig,
                DistributedRendererConfig = video?.DistributedRendererSettings,
                FractalBenchmarkConfig = benchmarkConfig,
            };

            if (Prompts.Confirm($"[{ThemeColor.Accent}]Save this configuration[/] to a JSON file for later reuse?"))
            {
                var path = Prompts.SavePath(
                    $"[{ThemeColor.Accent}]Where[/] should the file be saved to?", 
                    defaultValue: "config.json",
                    allowedFormats: [".json"]
                    );
                File.WriteAllText(path, JsonSerializer.Serialize(settings, FractalisJsonOptions.Default));
            }

            Prompts.Done();

            AnsiConsole.MarkupLine("[bold DarkOliveGreen2]Configuration complete[/]");
            Console.WriteLine();

            return settings;
        }

        #region Config methods
        private static FractalRendererConfig ConfigureRenderer(AppMode appMode, VideoMode? videoMode)
        {
            var fractal     = new FractalPhase().Run();
            var location    = new LocationPhase(fractal.Type, appMode == AppMode.Video).Run();
            var output      = new OutputPhase(appMode, videoMode).Run();
            var appearance  = new AppearancePhase().Run();
            var post        = new PostProcessingPhase().Run();

            return new FractalRendererConfig()
            {
                Fractal = IFractal.Create(fractal.Type, fractal.Parameters),
                Iterations = fractal.Iterations,
                Width = output.Resolution.Width,
                Height = output.Resolution.Height,
                Center = location.Center,
                Zoom = location.Zoom,
                ColorPalette = ColorPalette.FromPreset(appearance.Palette),
                AntiAliasing = appearance.AntiAliasing,
                ProcessorUsageLimit = output.ProcessorUsageLimit,
                LayerCompositor = post.Layers != null ? new LayerCompositor(post.Layers) : null,
            };
        }

        private static VideoPhaseResult ConfigureVideo() => new VideoPhase().Run();

        private static FractalBenchmarkConfig ConfigureBenchmark() => new BenchmarkPhase().Run();
        #endregion
    }
}
