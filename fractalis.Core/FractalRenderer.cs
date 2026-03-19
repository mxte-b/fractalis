using fractalis.Core.Fractals;
using fractalis.Core.Numbers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Spectre.Console;
using System.Collections.Concurrent;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

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
        private double                      _aspectRatio            = (double)config.Width / config.Height;
        private BigComplex                  _center                 = config.Center;
        private Complex                     _centerDouble           = config.Center.ToComplex();
        private FloatExp                    _pixelSpacing           = FloatExp.One / (FloatExp)config.Zoom;
        private double                      _pixelSpacingDouble     = 1 / config.Zoom.ToDouble();

        private static readonly FloatExp    HIGHPRECISION_THRESHOLD = new FloatExp(1, -40);
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

        public Image<Rgb24> Render(bool showProgress = true)
        {
            Image<Rgb24> image  = new Image<Rgb24>(Width, Height);
            RenderMode mode     = RenderMode;

            if (mode != RenderMode.Default && Fractal is IPerturbableFractal perturbable && _referenceOrbit.PointsR == null)
            {
                perturbable.CalculateReferenceOrbit(_center, Iterations, out _referenceOrbit);
            }

            Action<int> renderRow = (mode, Fractal) switch
            {
                (RenderMode.HighPrecision,             ISimdPerturbableFractal s) => y => RenderRowSimdPerturbed   (image, y, s),
                (RenderMode.HighPrecision,             IPerturbableFractal p)     => y => RenderRowPerturbed       (image, y, p),
                (RenderMode.HighPrecisionWithFloatExp, IPerturbableFractal p)     => y => RenderRowFloatExp        (image, y, p),
                (_,                                    ISimdFractal s)            => y => RenderRowSimd            (image, y, s),
                _                                                                 => y => RenderRowScalar          (image, y)
            };

            RenderRows(renderRow, showProgress, mode);
            return image;
        }

        // Helpers
        private double NdcY(int y)              => -(((double)y / Height) - 0.5) * 2.0;
        private double NdcX(int x)              => x * (2.0 / Height) - _aspectRatio;
        private Rgb24 Sample(IterationResult r)
        {
            if (!r.Escaped) return ColorPalette.InteriorColor.ToPixel<Rgb24>();

            double smoothIter = Fractal.GetContinousValue(r);

            System.Numerics.Vector4 c = ColorPalette.Sample(smoothIter);
            return new Rgb24((byte)(c.X * 255), (byte)(c.Y * 255), (byte)(c.Z * 255));
        }
        private Complex PixelCoordinates(double ndcX, double ndcY) => new (ndcX / _zoomDouble + _centerDouble.Real, ndcY / _zoomDouble + _centerDouble.Imaginary);

        // Row rendering
        private void RenderRowScalar(Image<Rgb24> image, int y)
        {
            double ndcY = NdcY(y);

            for (int x = 0; x < Width; x++)
            {
                double ndcX = NdcX(x);
                IterationResult result  = Fractal.Iteration(PixelCoordinates(ndcX, ndcY), Iterations);
                image[x, y] = Sample(result);
            }
        }

        private void RenderRowSimd(Image<Rgb24> image, int y, ISimdFractal simd)
        {
            double ndcY = NdcY(y);
            double ci = ndcY * _pixelSpacingDouble + _centerDouble.Imaginary;
            double ndcStepX = 2.0 / Height;

            int x = 0;
            for (; x <= Width - 4; x += 4)
            {
                var ndcX = Fma.MultiplyAdd(
                    Vector256.Create(x, x + 1.0, x + 2.0, x + 3.0),
                    Vector256.Create(ndcStepX),
                    Vector256.Create(-_aspectRatio));

                var cr = Fma.MultiplyAdd(ndcX, Vector256.Create(_pixelSpacingDouble), Vector256.Create(_centerDouble.Real));
                var (r0, r1, r2, r3) = simd.IterationSIMD(cr, Vector256.Create(ci), Iterations);

                image[x, y]     = Sample(r0);
                image[x + 1, y] = Sample(r1);
                image[x + 2, y] = Sample(r2);
                image[x + 3, y] = Sample(r3);
            }

            // If there are remaining pixels, just render normally
            for (; x < Width; x++)
            {
                double ndcX = NdcX(x);
                IterationResult result = Fractal.Iteration(PixelCoordinates(ndcX, ndcY), Iterations);
                image[x, y] = Sample(result);
            }
        }

        private void RenderRowPerturbed(Image<Rgb24> image, int y, IPerturbableFractal p)
        {
            double ndcY = NdcY(y);
            for (int x = 0; x < Width; x++)
            {
                double ndcX = NdcX(x);
                IterationResult result = p.IterationPerturbed(ndcX * _pixelSpacingDouble, ndcY * _pixelSpacingDouble, Iterations, in _referenceOrbit);
                image[x, y] = Sample(result);
            }
        }

        private void RenderRowFloatExp(Image<Rgb24> image, int y, IPerturbableFractal p)
        {
            double ndcY = NdcY(y);
            for (int x = 0; x < Width; x++)
            {
                double ndcX = NdcX(x);
                ScaledComplex delta = new ScaledComplex(ndcX * _pixelSpacing, ndcY * _pixelSpacing);
                IterationResult result = p.IterationFloatExp(delta, Iterations, in _referenceOrbit);
                image[x, y] = Sample(result);
            }
        }

        private void RenderRowSimdPerturbed(Image<Rgb24> image, int y, ISimdPerturbableFractal simd)
        {
            double ndcY = NdcY(y);
            double ndcStepX = 2.0 / Height;
            int x = 0;

            for (; x <= Width - 4; x += 4)
            {
                var ndcX = Fma.MultiplyAdd(
                    Vector256.Create(x, x + 1.0, x + 2.0, x + 3.0),
                    Vector256.Create(ndcStepX),
                    Vector256.Create(-_aspectRatio));

                var (r0, r1, r2, r3) = simd.IterationPerturbedSIMD(ndcX, ndcY, _pixelSpacingDouble, Iterations, in _referenceOrbit);
                
                image[x, y]     = Sample(r0);
                image[x + 1, y] = Sample(r1);
                image[x + 2, y] = Sample(r2);
                image[x + 3, y] = Sample(r3);
            }
            for (; x < Width; x++)
            {
                double ndcX = NdcX(x);
                IterationResult result = simd.IterationPerturbed(ndcX * _pixelSpacingDouble, ndcY * _pixelSpacingDouble, Iterations, in _referenceOrbit);
                image[x, y] = Sample(result);
            }
        }

        private void RenderRows(Action<int> renderRow, bool showProgress, RenderMode mode)
        {
            var rows = Partitioner.Create(Enumerable.Range(0, Height), EnumerablePartitionerOptions.NoBuffering);

            if (!showProgress) 
            { 
                Parallel.ForEach(rows, renderRow);
                return;
            }

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
                Parallel.ForEach(rows, y => 
                { 
                    renderRow(y); 
                    task.Increment(1); 
                });
            });
        }
    }
}