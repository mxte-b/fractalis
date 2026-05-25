using fractalis.Core.Fractals;
using fractalis.Core.Miscellaneous.Phases;
using Spectre.Console;
using System.Text.Json;

namespace fractalis.Core.Miscellaneous
{
    public static class AppConfigurator
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

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

        public static AppSettings Configure(string[] args)
        {
            AnsiConsole.MarkupLine($"[bold {ThemeColor.Title}]Welcome to the Fractalis Configurator![/]");
            AnsiConsole.WriteLine();

            var mode = PromptAppMode();
            var rendererConfig = ConfigureRenderer(mode == AppMode.Video);
            var video = mode == AppMode.Video ? ConfigureVideo() : null;

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
            };

            if (Prompts.Confirm("Save this configuration to a JSON file for later reuse?"))
            {
                var path = Prompts.Text("Save path:", defaultValue: "config.json");
                File.WriteAllText(path, JsonSerializer.Serialize(settings, JsonOptions));
                Prompts.Success($"Config saved to [cyan]{path}[/]");
            }

            return settings;
        }

        private static FractalRendererConfig ConfigureRenderer(bool isVideo)
        {
            var fractal     = new FractalPhase().Run();
            var location    = new LocationPhase(fractal.Type, isVideo).Run();
            var output      = new OutputPhase().Run();
            var appearance  = new AppearancePhase().Run();

            return new FractalRendererConfig()
            {
                Fractal = IFractal.Create(fractal.Type),
                Iterations = fractal.Iterations,
                Width = output.Resolution.Width,
                Height = output.Resolution.Height,
                Center = location.Sight.Location,
                Zoom = location.Zoom,
                ColorPalette = ColorPalette.FromPreset(appearance.Palette),
            };
        }

        private static VideoPhaseResult? ConfigureVideo()
        {
            return new VideoPhase().Run();
        }
    }
}
