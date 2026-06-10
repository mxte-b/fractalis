using fractalis.Core.Compositor.Layers;
using fractalis.Core.Compositor.Layers.Stylistic;
using fractalis.Core.Miscellaneous;
using Spectre.Console;

namespace fractalis.Core.Compositor.Configurators.Stylistic
{
    internal class AsciiArtLayerConfigurator : ILayerConfigurator
    {
        public Type TargetType => typeof(AsciiArtLayer);

        public CompositeLayer Configure()
        {
            var scale = Prompts.TextValidated<float>(
                $"[{ThemeColor.Accent}]Scale[/] of the characters?", 
                b => b > 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be greater than zero.[/]"),
                1
            );

            return new AsciiArtLayer(scale);
        }
    }
}
