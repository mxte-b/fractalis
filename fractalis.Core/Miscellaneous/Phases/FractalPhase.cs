using fractalis.Core.Fractals;
using Spectre.Console;

namespace fractalis.Core.Miscellaneous.Phases
{
    public record FractalPhaseResult(FractalType Type, int Iterations);

    public class FractalPhase : IPromptPhase<FractalPhaseResult>
    {
        public FractalPhaseResult Run()
        {
            Prompts.Section("Fractal", 1);

            var type = Prompts.Selection(
                $"What [{ThemeColor.Accent}]fractal[/] would you like to render?",
                Enum.GetValues<FractalType>());

            var iterations = PromptIterations();

            Prompts.Done();
            return new(type, iterations);
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
