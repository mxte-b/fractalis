using fractalis.Core.Fractals;
using fractalis.Core.Fractals.Configurators;
using Spectre.Console;
using System.Reflection;

namespace fractalis.Core.Miscellaneous.Phases
{
    public record FractalPhaseResult(FractalType Type, FractalParameters Parameters, int Iterations);

    public class FractalPhase : IPromptPhase<FractalPhaseResult>
    {
        // Get all configurators in the application using reflection.
        // This approach ensures the extensibility of the fractals.
        private static readonly Dictionary<FractalType, IFractalConfigurator> _configurators =
            Assembly.GetExecutingAssembly()
                .GetTypes()

                // Select all types that implement IFractalConfigurator and instantiate
                .Where(t => !t.IsAbstract && !t.IsInterface && t.IsAssignableTo(typeof(IFractalConfigurator)))
                .Select(t => (IFractalConfigurator)Activator.CreateInstance(t)!)

                // Convert to a dictionary so that we can access it
                .ToDictionary(t => t.TargetType);

        public FractalPhaseResult Run()
        {
            Prompts.Section("Fractal");

            var type = Prompts.Selection(
                $"What [{ThemeColor.Accent}]fractal[/] would you like to render?",
                Enum.GetValues<FractalType>());

            var parameters = PromptParameters(type);
            var iterations = PromptIterations();

            Prompts.Done();
            return new(type, parameters, iterations);
        }

        private FractalParameters PromptParameters(FractalType type)
        {
            if (!_configurators.TryGetValue(type, out var configurator))
                throw new InvalidOperationException($"There is no configurator defined for '{type}'");

            return configurator.Configure();
        }

        private static int PromptIterations()
        {
            var preset = Prompts.Selection(
                $"What should the iteration [{ThemeColor.Accent}]depth[/] be?",
                Iteration.IterationPresets,
                converter: p => p.Name);

            if (preset.Value >= 0)
                return preset.Value;

            return Prompts.TextValidated<int>(
                "Enter iteration count:",
                n => n > 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be greater than zero.[/]"));
        }
    }
}
