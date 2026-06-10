using fractalis.Core.Compositor.Layers;

namespace fractalis.Core.Compositor.Configurators
{
    internal interface ILayerConfigurator
    {
        /// <summary>
        /// The <see cref="Type"/> handle of the composite layer that is configured.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Configures the associated layer using CLI prompts.
        /// </summary>
        /// <returns>An instance of the associated layer.</returns>
        public CompositeLayer Configure();
    }
}
