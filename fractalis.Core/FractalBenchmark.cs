using fractalis.Core.Fractals;
using fractalis.Core.Numbers;
using Spectre.Console;
using System.Diagnostics;
using System.Reflection;

namespace fractalis.Core
{
    public class FractalBenchmark(FractalRendererConfig config)
    {
        private readonly FractalRendererConfig _config = config;
        private static readonly FieldInfo _orbitField = typeof(FractalRenderer).GetField("_referenceOrbit", BindingFlags.NonPublic | BindingFlags.Instance)!;

        public void Run(string label, int runs = 3)
        {
            if (_config.Fractal is not IPerturbableFractal perturbable)
            {
                throw new InvalidOperationException("Fractal does not support perturbation.");
            }

            List<long> refTimes     = new List<long>();
            List<long> renderTimes  = new List<long>();

            for (int i = 0; i < runs; i++)
            {
                // Benchmarking reference orbit calculation
                Stopwatch refSw = Stopwatch.StartNew();
                perturbable.CalculateReferenceOrbit(_config.Center, _config.Iterations, out var orbit);
                refSw.Stop();
                refTimes.Add(refSw.ElapsedMilliseconds);

                // Little hack: setting the private field _referenceOrbit
                FractalRenderer renderer = new FractalRenderer(_config);
                _orbitField.SetValue(renderer, orbit);

                // Benchmarking reference orbit calculation
                Stopwatch renderSw = Stopwatch.StartNew();
                renderer.Render(showProgress: false);
                renderSw.Stop();
                renderTimes.Add(renderSw.ElapsedMilliseconds);
            }

            long avgRef = refTimes.Sum() / runs;
            long avgRender = renderTimes.Sum() / runs;
            long avgTotal = avgRef + avgRender;

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