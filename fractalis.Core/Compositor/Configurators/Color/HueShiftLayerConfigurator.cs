using fractalis.Core.Compositor.Layers;
using fractalis.Core.Compositor.Layers.Color;
using fractalis.Core.Miscellaneous;

namespace fractalis.Core.Compositor.Configurators.Color
{
    internal class HueShiftLayerConfigurator : ILayerConfigurator
    {
        public Type TargetType => typeof(HueShiftLayer);

        public CompositeLayer Configure()
        {
            var shift = Prompts.Text<float>($"Desired [{ThemeColor.Accent}]shift amount[/] (in degrees)?");

            return new HueShiftLayer(shift);
        }
    }
}
