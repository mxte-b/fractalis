using fractalis.Core.Compositor.Layers;
using fractalis.Core.Compositor.Layers.Color;
using fractalis.Core.Miscellaneous;
using Spectre.Console;

namespace fractalis.Core.Compositor.Configurators.Color
{
    internal class SaturationLayerConfigurator : ILayerConfigurator
    {
        public Type TargetType => typeof(SaturationLayer);

        public CompositeLayer Configure()
        {
            var saturation = Prompts.TextValidated<float>(
                $"Desired [{ThemeColor.Accent}]saturation[/]?",
                b => b >= 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be non-negative.[/]"),
                1);

            return new SaturationLayer(saturation);
        }
    }
}
