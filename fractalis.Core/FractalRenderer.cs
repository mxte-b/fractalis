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
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
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
        public double[]             PointsR         = new double[maxIterations];
        public double[]             PointsI         = new double[maxIterations];
        public ScaledComplex[]      ScaledPoints    = new ScaledComplex[maxIterations];
        public int                  EscapeIteration = 0;
    }

    public class FractalRenderer(FractalRendererConfig config)
    {
        private ReferenceOrbit              _referenceOrbit;
        private BigFloat                    _zoom                   = config.Zoom;
        private double                      _zoomDouble             = config.Zoom.ToDouble();
        private BigComplex                  _center                 = config.Center;
        private Complex                     _centerDouble           = config.Center.ToComplex();
        private FloatExp                    _pixelSpacing           = FloatExp.One / (FloatExp)config.Zoom;
        private double                      _pixelSpacingDouble     = 1 / config.Zoom.ToDouble();

        private static readonly FloatExp    HIGHPRECISION_THRESHOLD = new FloatExp(1, -53);
        private static readonly FloatExp    FLOATEXP_THRESHOLD      = new FloatExp(1, -1070);

        // --- Public properties ---
        public readonly IFractal            Fractal                 = config.Fractal;
        public readonly int                 Iterations              = config.Iterations;
        public readonly int                 Width                   = config.Width;
        public readonly int                 Height                  = config.Height;
        public readonly ColorPalette        ColorPalette            = config.ColorPalette;
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
        private double EvaluatePerturbation(double ndcX, double ndcY, int x, int y, IPerturbableFractal perturbable)
        {
            if (x == Width / 2 && y == Height / 2) return _referenceOrbit.EscapeIteration;

            IterationResult result;
            if (RenderMode == RenderMode.HighPrecision)
                result = perturbable.IterationPerturbed(ndcX * _pixelSpacingDouble, ndcY * _pixelSpacingDouble, Iterations, in _referenceOrbit);
            else
            {
                ScaledComplex dc = new ScaledComplex(ndcX * _pixelSpacing, ndcY * _pixelSpacing);
                result = perturbable.IterationFloatExp(dc, Iterations, in _referenceOrbit);
            }
            return Fractal.GetContinousValue(result);
        }

        public Image<Rgb24> Render(bool showProgress = true)
        {
            Image<Rgb24> image = new Image<Rgb24>(Width, Height);

            RenderMode mode = RenderMode;
            IPerturbableFractal? perturbable = Fractal as IPerturbableFractal;

            if (
                (mode == RenderMode.HighPrecision || mode == RenderMode.HighPrecisionWithFloatExp) &&
                _referenceOrbit.PointsR == null &&
                perturbable != null
            )
            {
                perturbable.CalculateReferenceOrbit(_center, Iterations, out _referenceOrbit);
                Console.WriteLine($"    - Done!");
            }

            double ndcScaleX = 2.0 * Width / Height;
            double ndcScaleY = 2.0;
            double ndcOffX = 0.5 * ndcScaleX;
            double ndcOffY = 0.5 * ndcScaleY;

            void RenderRow(int y)
            {
                double ndcY = -(((double)y / Height) - 0.5) * ndcScaleY;
                double ndcStepX = ndcScaleX / Width;

                if (mode == RenderMode.HighPrecision && perturbable != null)
                {
                    int x = 0;

                    for (; x <= Width - 4; x += 4)
                    {
                        Vector256<double> ndcX = Fma.MultiplyAdd(
                            Vector256.Create(x, x + 1.0, x + 2.0, x + 3.0),
                            Vector256.Create(ndcStepX),
                            Vector256.Create(-ndcOffX)
                        );

                        var (r0, r1, r2, r3) = perturbable.IterationPerturbedSIMD(ndcX, ndcY, _pixelSpacingDouble, Iterations, in _referenceOrbit);

                        image[x, y] = ColorPalette.Sample(Fractal.GetContinousValue(r0)).ToPixel<Rgb24>();
                        image[x + 1, y] = ColorPalette.Sample(Fractal.GetContinousValue(r1)).ToPixel<Rgb24>();
                        image[x + 2, y] = ColorPalette.Sample(Fractal.GetContinousValue(r2)).ToPixel<Rgb24>();
                        image[x + 3, y] = ColorPalette.Sample(Fractal.GetContinousValue(r3)).ToPixel<Rgb24>();
                    }

                    // If there are remaining pixels, just render normally
                    for (; x < Width; x++)
                    {
                        double ndcX = x * ndcStepX - ndcOffX;
                        double val = EvaluatePerturbation(ndcX, ndcY, x, y, perturbable);
                        image[x, y] = ColorPalette.Sample(val).ToPixel<Rgb24>();
                    }
                }
                else
                {
                    for (int x = 0; x < Width; x++)
                    {
                        double ndcX = x * ndcStepX - ndcOffX;
                        double val = mode == RenderMode.Default
                            ? EvaluateNormal(ndcX, ndcY)
                            : EvaluatePerturbation(ndcX, ndcY, x, y, perturbable!);
                        image[x, y] = ColorPalette.Sample(val).ToPixel<Rgb24>();
                    }
                }
            }

            if (showProgress) 
            {
                Console.WriteLine($"<#> Current render mode: {mode}");
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
                           RenderRow(y);
                           task.Increment(1);
                       });
                   });

                Console.WriteLine($"    - Done!");
            }
            else
            {
                Parallel.ForEach(Partitioner.Create(Enumerable.Range(0, Height), EnumerablePartitionerOptions.NoBuffering), RenderRow);
            }

            return image;
        }
    }
}