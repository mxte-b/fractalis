using fractalis.Core.Fractals;
using fractalis.Core.Miscellaneous;
using fractalis.Core.Renderers;
using Spectre.Console;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace fractalis.Core
{
    public record BenchmarkResult
    {
        public required string  Label   { get; init; }
        public required int     Runs    { get; init; }
        public required TimedResult? ReferenceOrbit { get; init; }
        public required TimedResult Render { get; init; }
    }

    public record TimedResult
    {
        public required float Average { get; init; }
        public required float Minimum { get; init; }
        public required float Maximum { get; init; }
        public required float[] Times { get; init; }
    }

    /// <summary>
    /// Provides benchmarking utilities for measuring fractal rendering performance.
    /// </summary>
    /// <remarks>
    /// Measures both reference orbit calculation and rendering time, and displays
    /// aggregated statistics using a formatted console table.
    /// </remarks>
    public class FractalBenchmark(FractalRendererConfig config)
    {
        private readonly FractalRendererConfig _config = config;
        private BenchmarkResult? _result = null;

        /// <summary>
        /// Reflection handle to the private <c>_referenceOrbit</c> field of <see cref="FractalRenderer"/>.
        /// </summary>
        /// <remarks>
        /// Used to inject a precomputed reference orbit into the renderer for benchmarking purposes.
        /// </remarks>
        private static readonly FieldInfo _orbitField = typeof(FractalRenderer).GetField("_referenceOrbit", BindingFlags.NonPublic | BindingFlags.Instance)!;

        /// <summary>
        /// Runs the benchmark for the configured fractal.
        /// </summary>
        /// <param name="label">A label displayed in the benchmark output table.</param>
        /// <param name="runs">Number of benchmark iterations to perform.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the configured fractal does not support perturbation.
        /// </exception>
        public void Run(string label, int runs = 3)
        {
            bool benchmarkOrbit = _config.Fractal is IPerturbableFractal
                && new FractalRenderer(_config).RenderMode != RenderMode.Default;

            List<long> refTimes     = [];
            List<long> renderTimes  = [];

            for (int i = 0; i < runs; i++)
            {
                ReferenceOrbit? orbit = null;

                if (benchmarkOrbit)
                {
                    var perturbable = (IPerturbableFractal)_config.Fractal;
                    Stopwatch refSw = Stopwatch.StartNew();
                    perturbable.CalculateReferenceOrbit(_config.Center, _config.Iterations, out var tempOrbit);
                    refSw.Stop();

                    orbit = tempOrbit;
                    refTimes.Add(refSw.ElapsedMilliseconds);
                }

                FractalRenderer renderer = new(_config);
                if (orbit is not null) _orbitField.SetValue(renderer, orbit);

                Stopwatch renderSw = Stopwatch.StartNew();
                renderer.Render(showProgress: false);
                renderSw.Stop();
                renderTimes.Add(renderSw.ElapsedMilliseconds);
            }

            BenchmarkResult result = new()
            {
                Label = label,
                Runs = runs,
                ReferenceOrbit = benchmarkOrbit ? ToTimedResult(refTimes) : null,
                Render = ToTimedResult(renderTimes),
            };

            Display(result);
            _result = result;
        }

        private static void Display(BenchmarkResult result)
        {
            Table table = new Table()
                .Border(TableBorder.Rounded)
                .Title($"[bold]{result.Label}[/]")
                .AddColumn("Phase")
                .AddColumn("Avg (ms)")
                .AddColumn("Min (ms)")
                .AddColumn("Max (ms)");

            if (result.ReferenceOrbit is { } refOrbit)
            {
                table.AddRow("Reference Orbit",
                    $"{refOrbit.Average:f2}",
                    $"{refOrbit.Minimum:f2}",
                    $"{refOrbit.Maximum:f2}");
            }

            table.AddRow("Render",
                $"{result.Render.Average:f2}",
                $"{result.Render.Minimum:f2}",
                $"{result.Render.Maximum:f2}");

            if (result.ReferenceOrbit is { } refOrbitTotal)
            {
                table.AddRow("[bold]Total[/]",
                    $"[bold]{refOrbitTotal.Average + result.Render.Average:f2}[/]",
                    $"{refOrbitTotal.Minimum + result.Render.Minimum:f2}",
                    $"{refOrbitTotal.Maximum + result.Render.Maximum:f2}");
            }

            AnsiConsole.Write(table);
            Console.WriteLine();
        }

        private static TimedResult ToTimedResult(List<long> times) => new()
        {
            Average = (float)times.Average(),
            Minimum = times.Min(),
            Maximum = times.Max(),
            Times = [.. times],
        };

        /// <summary>
        /// Prompts the user if they want to export the benchmark results, and exports the result when necessary.
        /// </summary>
        public void PromptSave()
        {
            if (_result is null) return;

            if (!Prompts.Confirm($"Do you want to [{ThemeColor.Accent}]export[/] the results?")) return;

            var outputPath = Prompts.SavePath(
                $"[{ThemeColor.Accent}]Where[/] should the results be saved to?",
                defaultValue: "benchmark.json",
                allowedFormats: [".json"]
                );
            File.WriteAllText(outputPath, JsonSerializer.Serialize(_result, FractalisJsonOptions.Default));
            Prompts.Done();
        }
    }
}