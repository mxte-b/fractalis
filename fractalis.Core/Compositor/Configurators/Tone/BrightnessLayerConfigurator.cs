using fractalis.Core.Compositor.Layers;
using fractalis.Core.Compositor.Layers.Tone;
using fractalis.Core.Miscellaneous;
using Spectre.Console;

namespace fractalis.Core.Compositor.Configurators.Tone
{
    internal class BrightnessLayerConfigurator : ILayerConfigurator
    {
        public Type TargetType => typeof(BrightnessLayer);

        public CompositeLayer Configure()
        {
            var brightness = Prompts.TextValidated<float>(
                $"Desired [{ThemeColor.Accent}]brightness[/]?",
                b => b >= 0 
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be non-negative.[/]"),
                1);

            return new BrightnessLayer(brightness);
        }
    }
}
