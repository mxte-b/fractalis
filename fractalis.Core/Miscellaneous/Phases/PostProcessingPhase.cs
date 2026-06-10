using fractalis.Core.Compositor.Configurators;
using fractalis.Core.Compositor.Layers;
using System.Reflection;
using System.Text.RegularExpressions;

namespace fractalis.Core.Miscellaneous.Phases
{
    public record PostProcessingPhaseResult(List<CompositeLayer>? Layers);

    internal class PostProcessingPhase : IPromptPhase<PostProcessingPhaseResult>
    {
        // Get all configurators in the application using reflection.
        // This approach ensures the extensibility of the compositor layers.
        private static readonly Dictionary<Type, ILayerConfigurator> _configurators =
            Assembly.GetExecutingAssembly()
                .GetTypes()

                // Select all types that implement ILayerConfigurator and instantiate
                .Where(t => !t.IsAbstract && !t.IsInterface && t.IsAssignableTo(typeof(ILayerConfigurator)))
                .Select(t => (ILayerConfigurator)Activator.CreateInstance(t)!)

                // Convert to a dictionary so that we can access it
                .ToDictionary(t => t.TargetType);

        /// <summary>
        /// Configures a composite layer of a given type.
        /// </summary>
        /// <param name="type">The type of the layer to configure</param>
        /// <returns>The configured layer.</returns>
        /// <exception cref="InvalidOperationException">If the type doesn't have a configurator</exception>
        private static CompositeLayer ConfigureLayer(Type type)
        {
            if (!_configurators.TryGetValue(type, out var configurator)) 
                throw new InvalidOperationException($"There is no configurator defined for '{type}'");

            return configurator.Configure();
        }

        private static bool ConfirmNewLayer() =>
            Prompts.Selection(
                $"Layer added. [{ThemeColor.Accent}]Add another[/]?",
                ["Add new layer", "Done"])
            .Convert(choice => choice == "Add new layer");

        private static string GetLayerName(Type t) => 
            Regex.Replace(t.Name.Replace("Layer", ""), "(?<=[a-z])(?=[A-Z])", " ");

        public PostProcessingPhaseResult Run()
        {
            Prompts.Section("Post-processing");

            if (!Prompts.Confirm($"Do you want to apply [{ThemeColor.Accent}]post-processing[/] effects?"))
            {
                Prompts.Done();
                return new(null);
            }

            // Configure layers until the user selects the "Done" option.
            List<CompositeLayer> layers = [];
            do
            {
                // Prompt for the layer type to add
                var layerType = Prompts.Selection(
                    $"Select the [{ThemeColor.Accent}]layer[/] to add:",
                    CompositeLayer.AllLayers.OrderBy(x => x.Name),
                    layerType => GetLayerName(layerType),
                    true
                );

                // Then configure the layer
                layers.Add(ConfigureLayer(layerType));
            }
            while (ConfirmNewLayer());

            Prompts.Done();
            return new(layers);
        }
    }
}
