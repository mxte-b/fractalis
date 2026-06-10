using fractalis.Core.Fractals;
using fractalis.Core.Miscellaneous.Phases;
using fractalis.Core.Compositor;
using fractalis.Core.Renderers;
using Spectre.Console;
using System.Text.Json;

namespace fractalis.Core.Miscellaneous
{
    public static class AppConfigurator
    {

        private static AppMode PromptAppMode() 
            => Prompts.Selection($"What would you like to [bold {ThemeColor.Accent}]do[/]?", 
                ["Image rendering", "Video rendering", "Benchmarking"])
            .Convert(choice => choice switch
            {
                "Image rendering" => AppMode.Image,
                "Video rendering" => AppMode.Video,
                "Benchmarking" => AppMode.Benchmark,
                _ => AppMode.Image
            });

        private static AppSettings? ParseConfig(string path) => JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path), FractalisJsonOptions.Default);

        private static AppSettings? TryLoadConfig(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--config" && i + 1 < args.Length)
                {
                    string path = args[i + 1];

                    Prompts.Info($"Trying to load config from path {path}");
                    return ParseConfig(path);
                }
            }

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
            var video = mode == AppMode.Video ? ConfigureVideo() : null;
            var rendererConfig = ConfigureRenderer(mode, video?.VideoMode);
            var benchmarkConfig = mode == AppMode.Benchmark ? ConfigureBenchmark() : null;

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold DarkOliveGreen2]Configuration complete[/]");
            AnsiConsole.WriteLine();

            AppSettings settings = new()
            {
                Mode = mode,
                FractalRendererConfig = rendererConfig,
                VideoMode = video?.VideoMode,
                VideoConfig = video?.VideoConfig,
                DistributedRendererSettings = video?.DistributedRendererSettings,
                FractalBenchmarkConfig = benchmarkConfig,
            };

            if (Prompts.Confirm("Save this configuration to a JSON file for later reuse?"))
            {
                var path = Prompts.Text("Save path:", defaultValue: "config.json");
                File.WriteAllText(path, JsonSerializer.Serialize(settings, FractalisJsonOptions.Default));
                Prompts.Success($"Config saved to [cyan]{path}[/]");
            }

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
                Fractal = IFractal.Create(fractal.Type),
                Iterations = fractal.Iterations,
                Width = output.Resolution.Width,
                Height = output.Resolution.Height,
                Center = location.Sight.Location,
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
