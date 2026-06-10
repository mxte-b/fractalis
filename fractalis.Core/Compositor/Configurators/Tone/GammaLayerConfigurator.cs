using fractalis.Core.Compositor.Layers;
using fractalis.Core.Compositor.Layers.Tone;
using fractalis.Core.Miscellaneous;

namespace fractalis.Core.Compositor.Configurators.Tone
{
    internal class GammaLayerConfigurator : ILayerConfigurator
    {
        public Type TargetType => typeof(GammaLayer);

        public CompositeLayer Configure()
        {
            var gamma = Prompts.Text<float>($"Desired [{ThemeColor.Accent}]gamma[/] value?", 1);

            return new GammaLayer(gamma);
        }
    }
}
