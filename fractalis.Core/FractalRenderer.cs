using fractalis.Core.Fractals;
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
        HighPrecision,
        HighPrecisionWithFloatExp
    }

    public struct ReferenceOrbit
    {
        public Complex[] Points;
        public int EscapeIteration;

        public ReferenceOrbit(int maxIterations)
        {
            Points = new Complex[maxIterations];
            EscapeIteration = 0;
        }
    }

    public class FractalRenderer<TFractal> where TFractal : IFractal, new()
    {
        // --- Public properties ---
        public int          Iterations, Width, Height;
        public double       Zoom        { get; set; }
        public Complex      Center      { get; set; }
        public BigFixed     ZoomHigh    { get; set; }
        public BigComplex   CenterHigh  { get; set; }
        public RenderMode   RenderMode
        {
            get
            {
                if (Fractal is not IPerturbableFractal)
                {
                    return RenderMode.Default;
                }

                if (PixelSpacing < 1e-300)
                {
                    return RenderMode.HighPrecisionWithFloatExp;
                }
                else if (PixelSpacing < 1e-15)
                {
                    return RenderMode.HighPrecision;
                }
                else return RenderMode.Default;
            }
        }
        public ColorPalette ColorPalette { get; set; }

        // --- Private properties ---
        private ReferenceOrbit      ReferenceOrbit;
        private readonly TFractal   Fractal = new();
        private double              PixelSpacing {
            get
            {
                return 1 / (Width * Zoom);
            }
        }

        // --- Methods ---
        private double EvaluateNormal(double ndcX, double ndcY)
        {
            Complex c = new Complex(ndcX / Zoom + Center.Real, ndcY / Zoom + Center.Imaginary);

            var result = Fractal.Iteration(c, Iterations);
            return Fractal.GetContinousValue(result);
        }
        private double EvaluateHighPrecision(double ndcX, double ndcY, int x, int y)
        {
            if (x == Width / 2 && y == Height / 2)
                return ReferenceOrbit.EscapeIteration;

            if (Fractal is not IPerturbableFractal perturbable)
                throw new InvalidOperationException("Fractal does not support perturbation.");

            BigComplex dc = new BigComplex(ndcX / ZoomHigh, ndcY / ZoomHigh);

            var result = perturbable.IterationPerturbed(dc.ToComplex(), Iterations, in ReferenceOrbit);
            return Fractal.GetContinousValue(result);
        }

        private double EvaluateFloatExp(double ndcX, double ndcY, int x, int y)
        {
            if (x == Width / 2 && y == Height / 2)
                return ReferenceOrbit.EscapeIteration;

            if (Fractal is not IPerturbableFractal perturbable)
                throw new InvalidOperationException("Fractal does not support perturbation.");

            BigComplex dc = new BigComplex(ndcX / ZoomHigh, ndcY / ZoomHigh);

            var result = perturbable.IterationPerturbed(dc.ToComplex(), Iterations, in ReferenceOrbit);
            return Fractal.GetContinousValue(result);
        }
        private double Evaluate(double ndcX, double ndcY, int x, int y)
        {
            return RenderMode switch
            {
                RenderMode.Default => EvaluateNormal(ndcX, ndcY),
                RenderMode.HighPrecision => EvaluateHighPrecision(ndcX, ndcY, x, y),
                RenderMode.HighPrecisionWithFloatExp => EvaluateFloatExp(ndcX, ndcY, x, y),
                _ => throw new InvalidOperationException("Unknown render mode."),
            };
        }

        private Rgb24 ComputePixel(int x, int y)
        {
            double ndcX = (double)x / Width - 0.5;
            double ndcY = -((double)y / Height - 0.5);
            ndcX *= (double)Width / Height;

            double value = Evaluate(ndcX, ndcY, x, y);

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

        public FractalRenderer(int i, int w, int h, BigFixed z, BigComplex c)
        {
            Iterations = i;
            Width = w;
            Height = h;

            ZoomHigh = z;
            CenterHigh = c;

            Zoom = (double)z;
            Center = c.ToComplex();
        }
    }
}
