namespace fractalis.Core.Miscellaneous.Phases
{
    public class BenchmarkPhase : IPromptPhase<FractalBenchmarkConfig>
    {
        public FractalBenchmarkConfig Run()
        {
            Prompts.Section("Benchmark");

            var label = Prompts.Text<string>($"[{ThemeColor.Accent}]Label[/] of the benchmark?");
            var runs = Prompts.Text<int>($"[{ThemeColor.Accent}]Number of runs[/] in the benchmark?", 10);

            Prompts.Done();

            return new()
            {
                Label = label,
                Runs = runs
            };
        }
    }
}
