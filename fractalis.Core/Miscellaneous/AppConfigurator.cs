using fractalis.Core.Fractals;
using fractalis.Core.Numbers;
using Spectre.Console;

namespace fractalis.Core.Miscellaneous;

public static class AppConfigurator
{
    private static void Welcome()
    {
        AnsiConsole.MarkupLine("[bold yellow]Welcome to the Fractalis Configurator![/]");
    }

    private static AppMode PromptAppMode()
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .AddChoices("Image rendering", "Video rendering", "Benchmarking")
            .Title("What would you like to [green]do[/]?")
            );
            
        return choice switch
        {
            "Image rendering" => AppMode.Image,
            "Video rendering" => AppMode.Video,
            "Benchmarking" => AppMode.Benchmark,
            _ => AppMode.Image
        };
    }

    private static IFractal CreateFractal(FractalType type)
    {
        return type switch
        {
            FractalType.Mandelbrot => new Mandelbrot(),
            _ => throw new Exception("Unknown fractal type: " + type)
        };
    }

    private static FractalRendererConfig ConfigureRenderer()
    {
        var type = AnsiConsole.Prompt(
            new SelectionPrompt<FractalType>()
            .AddChoices(Enum.GetValues(typeof(FractalType)).Cast<FractalType>())
            .Title("What [green]fractal[/] would you like to render?")
        );

        var resolution = AnsiConsole.Prompt(
            new SelectionPrompt<ResolutionPreset>()
                .UseConverter(x => x.Name)
                .AddChoices(Resolution.CommonResolutions)
                .Title("What [green]resolution[/] should the renderer use?")).Resolution;

        var sight = AnsiConsole.Prompt(
            new SelectionPrompt<Sight>()
            .AddChoices(Sights.All.Where(x => x.FractalType == type))
            .UseConverter(x => x.Name)
            .EnableSearch()
            .Title("What [green]location[/] should be in the center?") );

        var palettePreset = AnsiConsole.Prompt(
            new SelectionPrompt<PalettePreset>()
                .Title("What [green]color palette[/] should the renderer use?")
                .AddChoices(Enum.GetValues(typeof(PalettePreset)).Cast<PalettePreset>()));

        var zoom = AnsiConsole.Prompt(
        new TextPrompt<BigFloat>("What should [green]zoom[/] level be?"));

        var iterations = AnsiConsole.Prompt<int>(
        new TextPrompt<int>("Number of fractal [green]iterations[/]:"));

        return new FractalRendererConfig()
        {
            Fractal = CreateFractal(type),
            Width = resolution.Width,
            Height = resolution.Height,
            Center = sight.Location,
            Zoom = zoom,
            Iterations = iterations,
            ColorPalette = ColorPalette.FromPreset(palettePreset),
        };
    }
    
    public static AppSettings Configure()
    {
        Welcome();
        
        var mode = PromptAppMode();
        
        var rendererConfig = ConfigureRenderer();
        
        if (mode != AppMode.Video) return new AppSettings()
        {
            Mode = mode,
            FractalRendererConfig =  rendererConfig
        };
        
        throw new NotImplementedException();
    }
}