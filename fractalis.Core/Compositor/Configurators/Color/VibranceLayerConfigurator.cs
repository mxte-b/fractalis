using fractalis.Core.Compositor.Layers;
using fractalis.Core.Compositor.Layers.Color;
using fractalis.Core.Miscellaneous;
using Spectre.Console;

namespace fractalis.Core.Compositor.Configurators.Color
{
    internal class VibranceLayerConfigurator : ILayerConfigurator
    {
        public Type TargetType => typeof(VibranceLayer);

        public CompositeLayer Configure()
        {
            var vibrance = Prompts.TextValidated<float>(
                $"Desired [{ThemeColor.Accent}]vibrance[/]?",
                b => b >= 0 && b <= 1
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be in the range of 0 to 1.[/]"));

            return new VibranceLayer(vibrance);
        }
    }
}
