using fractalis.Core.Fractals;
using fractalis.Core.Renderers;
using Spectre.Console;
using System.Diagnostics;
using System.Reflection;

namespace fractalis.Core
{
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
            if (_config.Fractal is not IPerturbableFractal perturbable)
            {
                throw new InvalidOperationException("Fractal does not support perturbation.");
            }

            List<long> refTimes     = [];
            List<long> renderTimes  = [];

            for (int i = 0; i < runs; i++)
            {
                // Benchmarking reference orbit calculation
                Stopwatch refSw = Stopwatch.StartNew();
                perturbable.CalculateReferenceOrbit(_config.Center, _config.Iterations, out var orbit);
                refSw.Stop();
                refTimes.Add(refSw.ElapsedMilliseconds);

                // Little hack: setting the private field _referenceOrbit
                FractalRenderer renderer = new(_config);
                _orbitField.SetValue(renderer, orbit);

                // Benchmarking rendering phase
                Stopwatch renderSw = Stopwatch.StartNew();
                renderer.Render(showProgress: false);
                renderSw.Stop();
                renderTimes.Add(renderSw.ElapsedMilliseconds);
            }

            long avgRef = refTimes.Sum() / runs;
            long avgRender = renderTimes.Sum() / runs;
            long avgTotal = avgRef + avgRender;

            /// <summary>
            /// Displays benchmark results in a formatted table.
            /// </summary>
            Table table = new Table()
                .Border(TableBorder.Rounded)
                .Title($"[bold]{label}[/]")
                .AddColumn("Phase")
                .AddColumn("Avg (ms)")
                .AddColumn("Min (ms)")
                .AddColumn("Max (ms)");

            table.AddRow("Reference Orbit",
                avgRef.ToString(),
                refTimes.Min().ToString(),
                refTimes.Max().ToString());

            table.AddRow("Render",
                avgRender.ToString(),
                renderTimes.Min().ToString(),
                renderTimes.Max().ToString());

            table.AddRow("[bold]Total[/]",
                $"[bold]{avgTotal}[/]",
                (refTimes.Min() + renderTimes.Min()).ToString(),
                (refTimes.Max() + renderTimes.Max()).ToString());

            AnsiConsole.Write(table);
        }
    }
}