using fractalis.Core.Compositor;
using fractalis.Core.Distributed.Networking;
using fractalis.Core.Fractals;
using fractalis.Core.Numbers;
using fractalis.Core.Renderers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Spectre.Console;
using System.Collections.Concurrent;

namespace fractalis.Core
{
    /// <summary>
    /// Performs rendering of fractals to an <see cref="Image{Rgb24}"/> with support
    /// for scalar, SIMD, perturbation, and FloatExp modes.
    /// </summary>
    public partial class FractalRenderer(FractalRendererConfig config)
    {
        private ReferenceOrbit              _referenceOrbit;
        private BigFloat                    _zoom                   = config.Zoom;
        private double                      _zoomDouble             = config.Zoom.ToDouble();
        private readonly double             _aspectRatio            = (double)config.Width / config.Height;
        private readonly int                _aaSamples              = (int)config.AntiAliasing;
        private BigComplex                  _center                 = config.Center;
        private Complex                     _centerDouble           = config.Center.ToComplex();
        private FloatExp                    _pixelSpacing           = FloatExp.One / (FloatExp)config.Zoom;
        private double                      _pixelSpacingDouble     = 1 / config.Zoom.ToDouble();

        private const int                   AA_THRESHOLD            = 30;
        private static readonly FloatExp    HIGHPRECISION_THRESHOLD = new(1, -40);
        private static readonly FloatExp    FLOATEXP_THRESHOLD      = new(1, -1024);

        /// <summary>The fractal to render.</summary>
        public readonly IFractal            Fractal                 = config.Fractal;

        /// <summary>Maximum iterations for escape-time calculation.</summary>
        public readonly int                 Iterations              = config.Iterations;

        /// <summary>Width of the output image.</summary>
        public readonly int                 Width                   = config.Width;

        /// <summary>Height of the output image.</summary>
        public readonly int                 Height                  = config.Height;

        /// <summary>Color palette used for rendering.</summary>
        public readonly ColorPalette        ColorPalette            = config.ColorPalette;

        /// <summary>The layer compositor used for post-processing.</summary>
        public readonly LayerCompositor?    LayerCompositor         = config.LayerCompositor;

        /// <summary>Zoom level for the fractal view.</summary>
        public BigFloat                     Zoom            
        {
            get => _zoom;
            set
            {
                _zoom = value;
                _zoomDouble = value.ToDouble();
                _pixelSpacing = FloatExp.One / (FloatExp)value;
                _pixelSpacingDouble = 1 / _zoomDouble;
            }
        }

        /// <summary>Automatically determines the appropriate render mode based on precision thresholds.</summary>
        public RenderMode                   RenderMode
        {
            get
            {
                if (Fractal is not IPerturbableFractal)
                {
                    return RenderMode.Default;
                }

                if (_pixelSpacing < FLOATEXP_THRESHOLD)
                {
                    return RenderMode.HighPrecisionWithFloatExp;
                }
                else if (_pixelSpacing < HIGHPRECISION_THRESHOLD)
                {
                    return RenderMode.HighPrecision;
                }
                else return RenderMode.Default;
            }
        }
        
        /// <summary>Number of available CPU cores adhering to the maximum usage defined in the config.</summary>
        public int AvailableCores => (int)Math.Max(Math.Round(Environment.ProcessorCount * config.ProcessorUsageLimit), 1);

        /// <summary>
        /// Renders the fractal to an image.
        /// </summary>
        /// <param name="showProgress">Whether to show a console progress bar.</param>
        /// <returns>The rendered image.</returns>
        public Image<Rgba32> Render(bool showProgress = true)
        {
            RenderMode mode = RenderMode;

            if (mode != RenderMode.Default && Fractal is IPerturbableFractal perturbable && _referenceOrbit.PointsR == null)
            {
                perturbable.CalculateReferenceOrbit(_center, Iterations, out _referenceOrbit);
            }

            Rgba32[] colorBuffer = new Rgba32[Width * Height];

            Action<int> renderRow = (mode, Fractal, SimdAgnostic.IsSupported) switch
            {
                (RenderMode.HighPrecision,             ISimdPerturbableFractal s, true)  => y => RenderRowSimdPerturbed(colorBuffer, y, s),
                (RenderMode.HighPrecision,             IPerturbableFractal p,     _)     => y => RenderRowPerturbed    (colorBuffer, y, p),
                (RenderMode.HighPrecisionWithFloatExp, IPerturbableFractal p,     _)     => y => RenderRowFloatExp     (colorBuffer, y, p),
                (_,                                    ISimdFractal s,            true)  => y => RenderRowSimd         (colorBuffer, y, s),
                _                                                                        => y => RenderRowScalar       (colorBuffer, y)
            };

            // First pass
            RenderRows(renderRow, showProgress, mode);

            // Second pass for anti-aliasing
            if (_aaSamples > 1)
            {
                var rows = Partitioner.Create(Enumerable.Range(0, Height), EnumerablePartitionerOptions.NoBuffering);
                var options = new ParallelOptions { MaxDegreeOfParallelism = AvailableCores };

                Rgba32[] aaBuffer = (Rgba32[])colorBuffer.Clone();

                Action<int> rowAA = (RenderMode, Fractal, SimdAgnostic.IsSupported && _aaSamples > 2) switch
                {
                    (RenderMode.HighPrecision,             ISimdPerturbableFractal s, true) => y => AAPassSimdPerturbed (colorBuffer, aaBuffer, y, s),
                    (RenderMode.HighPrecision,             IPerturbableFractal p,     _)    => y => AAPassPerturbed     (colorBuffer, aaBuffer, y, p),
                    (RenderMode.HighPrecisionWithFloatExp, IPerturbableFractal p,     _)    => y => AAPassFloatExp      (colorBuffer, aaBuffer, y, p),
                    (_,                                    ISimdFractal s,            true) => y => AAPassSimd          (colorBuffer, aaBuffer, y, s),
                    _                                                                       => y => AAPassScalar        (colorBuffer, aaBuffer, y)
                };

                //Parallel.ForEach(rows, options, row => rowAA(row));

                AAPass(rowAA, showProgress);

                colorBuffer = aaBuffer;
            }

            // Apply post-processing if necessary
            LayerCompositor?.Apply(colorBuffer, Width, Height);

            return Image.LoadPixelData<Rgba32>(colorBuffer, Width, Height);
        }

        private void RenderRows(Action<int> renderRow, bool showProgress, RenderMode mode)
        {
            var rows = Partitioner.Create(Enumerable.Range(0, Height), EnumerablePartitionerOptions.NoBuffering);
            var options = new ParallelOptions { MaxDegreeOfParallelism = AvailableCores };
            
            if (!showProgress) 
            { 
                Parallel.ForEach(rows, options, renderRow);
                return;
            }

            Console.WriteLine($"<#> Current render mode: {mode}{(SimdAgnostic.IsSupported ? " - SIMD accelerated" : "No acceleration")}");
            AnsiConsole.Progress()
            .Columns([
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new ElapsedTimeColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn(),
            ])
            .Start(ctx =>
            {
                var task = ctx.AddTask($"<#> Rendering", maxValue: Height);
                Parallel.ForEach(rows, options, y => 
                { 
                    renderRow(y); 
                    task.Increment(1); 
                });
            });
        }

        private void AAPass(Action<int> aaRow, bool showProgress)
        {
            var rows = Partitioner.Create(Enumerable.Range(0, Height), EnumerablePartitionerOptions.NoBuffering);
            var options = new ParallelOptions { MaxDegreeOfParallelism = AvailableCores };

            if (!showProgress)
            {
                Parallel.ForEach(rows, options, aaRow);
                return;
            }

            AnsiConsole.Progress()
            .Columns([
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new ElapsedTimeColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn(),
            ])
            .Start(ctx =>
            {
                var task = ctx.AddTask($"<#> AA Pass", maxValue: Height);
                Parallel.ForEach(rows, options, y =>
                {
                    aaRow(y);
                    task.Increment(1);
                });
            });
        }
    }
}