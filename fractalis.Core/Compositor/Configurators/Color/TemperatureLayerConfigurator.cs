using fractalis.Core.Compositor.Layers;
using fractalis.Core.Compositor.Layers.Color;
using fractalis.Core.Miscellaneous;
using Spectre.Console;

namespace fractalis.Core.Compositor.Configurators.Color
{
    internal class TemperatureLayerConfigurator : ILayerConfigurator
    {
        public Type TargetType => typeof(TemperatureLayer);

        public CompositeLayer Configure()
        {
            var saturation = Prompts.TextValidated<float>(
                $"Desired [{ThemeColor.Accent}]temperature[/] (1000K - 10000K)?",
                b => b >= 1000 && b <= 10000
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must in the range 1000 to 10000.[/]"),
                6500);

            return new SaturationLayer(saturation);
        }
    }
}
