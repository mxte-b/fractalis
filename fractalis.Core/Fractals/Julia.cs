using fractalis.Core.Numbers;
using Spectre.Console;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace fractalis.Core.Fractals
{
    [method: JsonConstructor]
    internal class Julia(BigComplex constant) : IPerturbableFractal
    {
        private readonly BigComplex _constant = constant;

        private readonly Complex _constantDouble = constant.ToComplex();

        /// <summary>
        /// Constant for converting logarithms to base 2.
        /// </summary>
        private const double ILOG2 = 1.4426950408889634;

        /// <summary>
        /// Maximum magnitude before a point is considered escaped in high-precision iterations.
        /// </summary>
        private static readonly FloatExp BAILOUT = new(1, 7);

        /// <summary>
        /// Maximum magnitude before a point is considered escaped in double-precision iterations.
        /// </summary>
        private static readonly double BAILOUT_DOUBLE = Math.Pow(2, 7);

        public Julia(JuliaParameters parameters) : this(parameters.Constant) { }

        public BigComplex Constant => _constant;

        #region Iterations
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public IterationResult Iteration(Complex z0, int maxIterations)
        {
            Complex z = z0;
            int i = 0;

            for (; i < maxIterations; i++)
            {
                double zrTemp = z.Real;

                z.Real = z.Real * z.Real - z.Imaginary * z.Imaginary + _constantDouble.Real;
                z.Imaginary = 2 * zrTemp * z.Imaginary + _constantDouble.Imaginary;

                if (z.MagnitudeSquared > BAILOUT_DOUBLE) break;
            }

            if (i == maxIterations) return new IterationResult(i, double.NaN);

            return new IterationResult(i, z.MagnitudeSquared);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe IterationResult IterationPerturbed(double dr, double di, int maxIterations, in ReferenceOrbit referenceOrbit)
        {
            int i = 0;
            int refIteration = 0;
            int escape = referenceOrbit.EscapeIteration - 1;
            double dzr = dr, dzi = di, zmag = 0;

            fixed (double* refR = referenceOrbit.PointsR)
            fixed (double* refI = referenceOrbit.PointsI)
            {
                double zr0 = refR[0];
                double zi0 = refI[0];

                for (; i < maxIterations; i++)
                {
                    double zr = refR[refIteration];
                    double zi = refI[refIteration];
                    refIteration++;

                    double ar = Math.FusedMultiplyAdd(2.0, zr, dzr);
                    double ai = Math.FusedMultiplyAdd(2.0, zi, dzi);

                    double newdzr = Math.FusedMultiplyAdd(ar, dzr, -ai * dzi);
                    dzi = Math.FusedMultiplyAdd(ar, dzi, ai * dzr);
                    dzr = newdzr;

                    zr = refR[refIteration] + dzr;
                    zi = refI[refIteration] + dzi;
                    zmag = Math.FusedMultiplyAdd(zr, zr, zi * zi);

                    // Bailout
                    if (zmag > BAILOUT_DOUBLE) break;

                    if (zmag < Math.FusedMultiplyAdd(dzr, dzr, dzi * dzi) || refIteration == escape)
                    {
                        dzr = zr - zr0;
                        dzi = zi - zi0;
                        refIteration = 0;
                    }
                }
            }

            if (i == maxIterations) return new IterationResult(i, double.NaN);
            return new IterationResult(i, zmag);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public IterationResult IterationFloatExpPerturbed(ScaledComplex delta, int maxIterations, in ReferenceOrbit referenceOrbit)
        {
            int i = 0;
            int refIteration = 0;
            ScaledComplex dz = delta;
            double escapeMag = 0;

            ScaledComplex z0 = referenceOrbit.ScaledPoints[0];

            for (; i < maxIterations; i++)
            {
                // We don't collect the common dz term here, because that would involve
                // adding the reference (~1e0 scale) to dz (~1e-300+ scale), which would be catastrophic.
                dz = 2 * referenceOrbit.ScaledPoints[refIteration++] * dz + dz * dz;

                ScaledComplex z = referenceOrbit.ScaledPoints[refIteration] + dz;
                FloatExp zMag = z.MagnitudeSquared;

                // Bailout
                if (zMag > BAILOUT)
                {
                    escapeMag = (double)zMag;
                    break;
                }

                // Prevent delta from straying off and causing visual glitches
                if (zMag < dz.MagnitudeSquared || refIteration == referenceOrbit.EscapeIteration - 1)
                {
                    dz = z - z0;
                    refIteration = 0;
                }
            }

            if (i == maxIterations) return new IterationResult(i, double.NaN);

            return new IterationResult(i, escapeMag);
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void CalculateReferenceOrbit(BigComplex center, int maxIterations, out ReferenceOrbit orbit)
        {
            ReferenceOrbit o = new(maxIterations);
            BigComplex z = center;
            int i = 0;

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
                    var task = ctx.AddTask("<#> Calculating reference orbit".PadRight(31), maxValue: maxIterations);

                    for (; i < maxIterations; i++)
                    {
                        Complex zc = z.ToComplex();
                        o.PointsR[i] = zc.Real;
                        o.PointsI[i] = zc.Imaginary;
                        o.ScaledPoints[i] = z.ToScaledComplex();

                        BigFloat zrTemp = z.Real;
                        BigFloat zr2 = z.Real * z.Real;
                        BigFloat zi2 = z.Imaginary * z.Imaginary;

                        z.Real = zr2 - zi2 + _constant.Real;
                        z.Imaginary = 2 * zrTemp * z.Imaginary + _constant.Imaginary;

                        if (zr2 + zi2 > BAILOUT_DOUBLE) break;

                        task.Increment(1);
                    }
                });

            o.EscapeIteration = i;
            orbit = o;
        }

        /// <summary>
        /// Computes a continuous iteration value for smooth coloring of fractal images.
        /// </summary>
        /// <param name="result">The iteration result from a Mandelbrot calculation.</param>
        /// <returns>A double representing the continuous iteration value for coloring.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetContinousValue(IterationResult result)
        {
            if (!result.Escaped) return result.Iteration;
            return result.Iteration + 1 - Math.Log(Math.Log(result.MagnitudeSquared) * 0.5) * ILOG2;
        }
    }
}
