using fractalis.Core.Fractals;
using fractalis.Core.Numbers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Spectre.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core
{
    public record FractalRendererConfig
    {
        public required IFractal    Fractal         { get; init; }
        public required int         Iterations      { get; init; }
        public required int         Width           { get; init; }
        public required int         Height          { get; init; }
        public required BigFloat    Zoom            { get; init; }
        public required BigComplex  Center          { get; init; }
        public ColorPalette         ColorPalette    { get; init; }
    }

    public enum RenderMode
    {
        Default,
        HighPrecision,              // Perturbation Theory
        HighPrecisionWithFloatExp   // FloatExp + Perturbation Theory
    }
    public struct ReferenceOrbit(int maxIterations)
    {
        public Complex[]            Points          = new Complex[maxIterations];
        public ScaledComplex[]      ScaledPoints    = new ScaledComplex[maxIterations];
        public int                  EscapeIteration = 0;
    }

    public class FractalRenderer(FractalRendererConfig config)
    {
        private ReferenceOrbit              _referenceOrbit;
        private BigFloat                    _zoom           = config.Zoom;
        private double                      _zoomDouble     = config.Zoom.ToDouble();
        private BigComplex                  _center         = config.Center;
        private Complex                     _centerDouble   = config.Center.ToComplex();
        private FloatExp                    _pixelSpacing   = FloatExp.One / (FloatExp)config.Zoom;

        private static readonly FloatExp    HIGHPRECISION_THRESHOLD = new FloatExp(1, -53);
        private static readonly FloatExp    FLOATEXP_THRESHOLD = new FloatExp(1, -1070);

        // --- Public properties ---
        public readonly IFractal            Fractal         = config.Fractal;
        public readonly int                 Iterations      = config.Iterations;
        public readonly int                 Width           = config.Width;
        public readonly int                 Height          = config.Height;
        public readonly ColorPalette        ColorPalette    = config.ColorPalette;
        public BigFloat                     Zoom            
        {
            get => _zoom;
            set
            {
                _zoom = value;
                _zoomDouble = value.ToDouble();
                _pixelSpacing = FloatExp.One / (FloatExp)value;
            }
        }
        public BigComplex                   Center          
        {
            get => _center;
            set
            {
                _center = value;
                _centerDouble = value.ToComplex();
            }
        }
        public FloatExp                     PixelSpacing
        {
            get => _pixelSpacing;
        }
        public RenderMode                   RenderMode
        {
            get
            {
                if (Fractal is not IPerturbableFractal)
                {
                    return RenderMode.Default;
                }

                if (PixelSpacing < FLOATEXP_THRESHOLD)
                {
                    return RenderMode.HighPrecisionWithFloatExp;
                }
                else if (PixelSpacing < HIGHPRECISION_THRESHOLD)
                {
                    return RenderMode.HighPrecision;
                }
                else return RenderMode.Default;
            }
        }

        // --- Methods ---
        private double EvaluateNormal(double ndcX, double ndcY)
        {
            Complex c = new Complex(ndcX / _zoomDouble + _centerDouble.Real, ndcY / _zoomDouble + _centerDouble.Imaginary);
            return Fractal.GetContinousValue(Fractal.Iteration(c, Iterations));
        }

        private double EvaluatePerturbation(double ndcX, double ndcY, int x, int y)
        {
            if (Fractal is not IPerturbableFractal perturbable) throw new InvalidOperationException("Fractal does not support perturbation.");

            // Since the reference point is at the center, that pixel is already calculated
            if (x == Width / 2 && y == Height / 2) return _referenceOrbit.EscapeIteration;

            ScaledComplex dc = new ScaledComplex(ndcX * _pixelSpacing, ndcY * _pixelSpacing);

            IterationResult result = RenderMode == RenderMode.HighPrecision
                ? perturbable.IterationPerturbed(dc.ToComplex(), Iterations, in _referenceOrbit)
                : perturbable.IterationFloatExp(dc, Iterations, in _referenceOrbit);

            return Fractal.GetContinousValue(result);
        }

        private Rgb24 ComputePixel(int x, int y)
        {
            double ndcX = (double)x / Width - 0.5;
            double ndcY = -((double)y / Height - 0.5);

            ndcY *= 2;
            ndcX *= (double)2 * Width / Height;

            double value = RenderMode switch
            {
                RenderMode.Default => EvaluateNormal(ndcX, ndcY),
                RenderMode.HighPrecision or RenderMode.HighPrecisionWithFloatExp => EvaluatePerturbation(ndcX, ndcY, x, y),
                _ => throw new InvalidOperationException("Invalid render mode.")
            };

            return ColorPalette.Sample(value).ToPixel<Rgb24>();
        }

        public Image<Rgb24> Render()
        {
            Image<Rgb24> image = new Image<Rgb24>(Width, Height);

            Console.WriteLine($"<#> Current render mode: {RenderMode}");

            // High Precision - using Perturbation Theory
            if (
                (RenderMode == RenderMode.HighPrecision || RenderMode == RenderMode.HighPrecisionWithFloatExp) &&
                _referenceOrbit.Points == null &&
                Fractal is IPerturbableFractal perturbable
            )
            {
                perturbable.CalculateReferenceOrbit(_center, Iterations, out _referenceOrbit);
                Console.WriteLine($"    - Done!");
            }

            // Main render loop
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

                Parallel.ForEach(Partitioner.Create(Enumerable.Range(0, Height), EnumerablePartitionerOptions.NoBuffering), y =>
                {
                    for (int x = 0; x < Width; x++) image[x, y] = ComputePixel(x, y);
                    task.Increment(1);
                });
            });

            Console.WriteLine($"    - Done!");
            return image;
        }
    }
}
