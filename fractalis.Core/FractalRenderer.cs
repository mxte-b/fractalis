using fractalis.Core.Fractals;
using fractalis.Core.Numbers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core
{
    public enum RenderMode
    {
        Default,
        HighPrecision, // Perturbation Theory
        HighPrecisionWithFloatExp // FloatExp + Perturbation Theory
    }

    public struct ReferenceOrbit
    {
        public Complex[] Points;
        public ScaledComplex[] ScaledPoints;
        public int EscapeIteration;

        public ReferenceOrbit(int maxIterations)
        {
            Points = new Complex[maxIterations];
            ScaledPoints = new ScaledComplex[maxIterations];
            EscapeIteration = 0;
        }
    }

    public class FractalRenderer<TFractal>(int i, int w, int h, BigFixed z, BigComplex c) where TFractal : IFractal, new()
    {
        // --- Public properties ---
        public int                  Iterations      = i;
        public int                  Width           = w;
        public int                  Height          = h;
        public double               Zoom            { get; set; } = (double)z;
        public Complex              Center          { get; set; } = c.ToComplex();
        public BigFixed             ZoomHigh        { get; set; } = z;
        public BigComplex           CenterHigh      { get; set; } = c;
        public RenderMode           RenderMode
        {
            get
            {
                if (Fractal is not IPerturbableFractal)
                {
                    return RenderMode.Default;
                }

                if (PixelSpacing < 1e-322)
                {
                    return RenderMode.HighPrecisionWithFloatExp;
                }
                else if (PixelSpacing < 1e-16)
                {
                    return RenderMode.HighPrecision;
                }
                else return RenderMode.Default;
            }
        }
        public ColorPalette         ColorPalette    { get; set; }

        // --- Private properties ---
        private ReferenceOrbit      ReferenceOrbit;
        private readonly TFractal   Fractal         = new();
        private double              PixelSpacing 
        {
            get
            {
                return 1 / (Width * Zoom);
            }
        }

        // --- Methods ---
        private double EvaluateNormal(double ndcX, double ndcY)
        {
            Complex c = new Complex(ndcX / Zoom + Center.Real, ndcY / Zoom + Center.Imaginary);
            return Fractal.Iteration(c, Iterations).Iteration;
        }

        private double EvaluatePerturbation(double ndcX, double ndcY, int x, int y)
        {
            if (Fractal is not IPerturbableFractal perturbable) throw new InvalidOperationException("Fractal does not support perturbation.");

            // Since the reference point is at the center, that pixel is already calculated
            if (x == Width / 2 && y == Height / 2) return ReferenceOrbit.EscapeIteration;

            BigComplex dc = new BigComplex(ndcX / ZoomHigh, ndcY / ZoomHigh);

            IterationResult result = RenderMode == RenderMode.HighPrecision
                ? perturbable.IterationPerturbed(dc.ToComplex(), Iterations, in ReferenceOrbit)
                : perturbable.IterationFloatExp(dc.ToScaledComplex(), Iterations, in ReferenceOrbit);

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
                Fractal is IPerturbableFractal perturbable
            )
            {
                perturbable.CalculateReferenceOrbit(CenterHigh, Iterations, out ReferenceOrbit);
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

                Parallel.For(0, Height, y =>
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
