using fractalis.Core.Numbers;
using Spectre.Console;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace fractalis.Core.Fractals
{
    public class Mandelbrot : IPerturbableFractal, ISimdPerturbableFractal
    {
        private const double                ILOG2           = 1.4426950408889634;
        private static readonly FloatExp    BAILOUT         = new FloatExp(1, 7);
        private static readonly double      BAILOUT_DOUBLE  = Math.Pow(2, 7);

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public IterationResult Iteration(Complex c, int maxIterations)
        {
            Complex z = new Complex(0, 0);
            int i = 0;

            for (; i < maxIterations; i++)
            {
                double zrTemp = z.Real;

                z.Real = z.Real * z.Real - z.Imaginary * z.Imaginary + c.Real;
                z.Imaginary = 2 * zrTemp * z.Imaginary + c.Imaginary;

                if (z.MagnitudeSquared > 100) break;
            }

            if (i == maxIterations) return new IterationResult(i, double.NaN, false);

            return new IterationResult(i, z.MagnitudeSquared);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public IterationResult IterationFloatExp(ScaledComplex delta, int maxIterations, in ReferenceOrbit referenceOrbit)
        {
            int i = 0;
            int refIteration = 0;
            ScaledComplex dz = new ScaledComplex(0, 0);
            double escapeMag = 0;

            for (; i < maxIterations; i++)
            {
                // We don't collect the common dz term here, because that would involve
                // adding the reference (~1e0 scale) to dz (~1e-300+ scale), which would be catastrophic.
                dz = 2 * referenceOrbit.ScaledPoints[refIteration++] * dz + dz * dz + delta;

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
                    dz = z;
                    refIteration = 0;
                }
            }

            if (i == maxIterations) return new IterationResult(i, double.NaN, false);

            return new IterationResult(i, escapeMag);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe (IterationResult r0, IterationResult r1, IterationResult r2, IterationResult r3) IterationSIMD(Vector256<double> cr, Vector256<double> ci, int maxIterations)
        {
            Vector256<double> zr            = Vector256<double>.Zero;
            Vector256<double> zi            = Vector256<double>.Zero;
            Vector256<double> iterations    = Vector256<double>.Zero;
            Vector256<double> escapeMags    = Vector256<double>.Zero;
            Vector256<double> active        = Vector256<double>.AllBitsSet;
            Vector256<double> one           = Vector256.Create(1.0);
            Vector256<double> two           = Vector256.Create(2.0);
            Vector256<double> bailout       = Vector256.Create(BAILOUT_DOUBLE);

            for (int i = 0; i < maxIterations; i++)
            {
                Vector256<double> newzi = Fma.MultiplyAdd(two * zr, zi, ci);
                Vector256<double> newzr = Fma.MultiplyAdd(zr, zr, Fma.MultiplyAddNegated(zi, zi, cr));

                // Only apply it to non-escaped points
                zr = Avx.BlendVariable(zr, newzr, active);
                zi = Avx.BlendVariable(zi, newzi, active);

                // Bailout if every point escaped
                Vector256<double> zmag          = Fma.MultiplyAdd(zr, zr, zi * zi);
                Vector256<double> prevActive    = active;

                active      = Avx.Compare(zmag, bailout, FloatComparisonMode.OrderedLessThanNonSignaling);
                escapeMags  = Avx.BlendVariable(escapeMags, zmag, Avx.AndNot(active, prevActive));

                if (Avx.TestZ(active, active)) break;

                // Increment iterations
                iterations = Avx.Add(iterations, Avx.And(active, one));
            }

            double* magnitudeBuffer = stackalloc double[4];
            double* iterationBuffer = stackalloc double[4];

            Avx.Store(magnitudeBuffer, escapeMags);
            Avx.Store(iterationBuffer, iterations);

            int i0 = (int)iterationBuffer[0];
            int i1 = (int)iterationBuffer[1];
            int i2 = (int)iterationBuffer[2];
            int i3 = (int)iterationBuffer[3];

            double z0 = magnitudeBuffer[0];
            double z1 = magnitudeBuffer[1];
            double z2 = magnitudeBuffer[2];
            double z3 = magnitudeBuffer[3];

            static IterationResult Make(int i, double z, int m) => new(i, z, i < m);

            return (Make(i0, z0, maxIterations), Make(i1, z1, maxIterations), Make(i2, z2, maxIterations), Make(i3, z3, maxIterations));
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe (IterationResult r0, IterationResult r1, IterationResult r2, IterationResult r3) IterationPerturbedSIMD(Vector256<double> ndcX, double ndcY, double pixelSpacing, int maxIterations, in ReferenceOrbit referenceOrbit)
        {
            Vector256<double> dzr           = Vector256<double>.Zero;
            Vector256<double> dzi           = Vector256<double>.Zero;
            Vector256<double> iterations    = Vector256<double>.Zero;
            Vector256<double> escapeMags    = Vector256<double>.Zero;
            Vector256<double> active        = Vector256<double>.AllBitsSet;
            Vector256<double> one           = Vector256.Create(1.0);
            Vector256<double> two           = Vector256.Create(2.0);
            Vector256<double> bailout       = Vector256.Create(BAILOUT_DOUBLE);
            Vector256<double> dr            = Vector256.Multiply(pixelSpacing, ndcX);
            Vector256<double> di            = Vector256.Create(ndcY * pixelSpacing);

            int escape = referenceOrbit.EscapeIteration - 1;

            // Get pointer to reference orbit, without GC
            fixed (double* refR = referenceOrbit.PointsR)
            fixed (double* refI = referenceOrbit.PointsI)
            {
                int refIteration = 0;

                for (int i = 0; i < maxIterations; i++)
                {
                    // Read out reference point
                    Vector256<double> zr_ref = Vector256.Create(refR[refIteration]);
                    Vector256<double> zi_ref = Vector256.Create(refI[refIteration]);
                    refIteration++;

                    // 2 * z_ref + delta
                    Vector256<double> ar = Fma.MultiplyAdd(two, zr_ref, dzr);
                    Vector256<double> ai = Fma.MultiplyAdd(two, zi_ref, dzi);

                    // delta = (2 * z_ref * dz) * dz + dz + delta
                    Vector256<double> newdzr = Fma.MultiplyAdd(ar, dzr, Fma.MultiplyAddNegated(ai, dzi, dr));
                    Vector256<double> newdzi = Fma.MultiplyAdd(ar, dzi, Fma.MultiplyAdd(ai, dzr, di));

                    // Only apply it to non-escaped points
                    dzr = Avx.BlendVariable(dzr, newdzr, active);
                    dzi = Avx.BlendVariable(dzi, newdzi, active);

                    // Calulate next z
                    // z = z_ref + dz
                    Vector256<double> zr            = Vector256.Create(refR[refIteration]) + dzr;
                    Vector256<double> zi            = Vector256.Create(refI[refIteration]) + dzi;
                    Vector256<double> zmag          = Fma.MultiplyAdd(zr, zr, zi * zi);
                    Vector256<double> prevActive    = active;

                    // Bailout if every point escaped
                    active = Avx.Compare(zmag, bailout, FloatComparisonMode.OrderedLessThanNonSignaling);

                    // Store magnitudes for points that just escaped
                    escapeMags = Avx.BlendVariable(escapeMags, zmag, Avx.AndNot(active, prevActive));
                    if (Avx.TestZ(active, active)) break;

                    // Prevent delta from straying off and causing visual glitches
                    bool needsRebase = false;

                    // For performance, only check glitches every 4th iteration
                    // NEEDS TESTING FOR GLITCHES
                    if ((i & 3) == 0)
                    {
                        Vector256<double> dzmag = Fma.MultiplyAdd(dzr, dzr, dzi * dzi);
                        Vector256<double> cmp = Avx.Compare(zmag, dzmag, FloatComparisonMode.OrderedLessThanNonSignaling);
                        needsRebase = !Avx.TestZ(cmp, cmp);
                    }

                    if (needsRebase || refIteration == escape)
                    {
                        dzr = zr; dzi = zi; refIteration = 0;
                    }

                    // Increment iterations
                    iterations = Avx.Add(iterations, Avx.And(active, one));
                }
            }

            double* magnitudeBuffer = stackalloc double[4];
            double* iterationBuffer = stackalloc double[4];

            Avx.Store(magnitudeBuffer, escapeMags);
            Avx.Store(iterationBuffer, iterations);

            int i0 = (int)iterationBuffer[0];
            int i1 = (int)iterationBuffer[1];
            int i2 = (int)iterationBuffer[2];
            int i3 = (int)iterationBuffer[3];

            double z0 = magnitudeBuffer[0];
            double z1 = magnitudeBuffer[1];
            double z2 = magnitudeBuffer[2];
            double z3 = magnitudeBuffer[3];

            static IterationResult Make(int i, double z, int m) => new(i, z, i < m);

            return (Make(i0, z0, maxIterations), Make(i1, z1, maxIterations), Make(i2, z2, maxIterations), Make(i3, z3, maxIterations));
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe IterationResult IterationPerturbed(double dr, double di, int maxIterations, in ReferenceOrbit referenceOrbit)
        {
            int i = 0;
            int refIteration = 0;
            int escape = referenceOrbit.EscapeIteration - 1;
            double dzr = 0, dzi = 0, zmag = 0;

            fixed (double* refR = referenceOrbit.PointsR)
            fixed (double* refI = referenceOrbit.PointsI)
            {
                for (; i < maxIterations; i++)
                {
                    double zr = refR[refIteration];
                    double zi = refI[refIteration];
                    refIteration++;

                    double ar = Math.FusedMultiplyAdd(2.0, zr, dzr);
                    double ai = Math.FusedMultiplyAdd(2.0, zi, dzi);

                    double newdzr = Math.FusedMultiplyAdd(ar, dzr, Math.FusedMultiplyAdd(-ai, dzi, dr));
                    dzi = Math.FusedMultiplyAdd(ar, dzi, Math.FusedMultiplyAdd(ai, dzr, di));
                    dzr = newdzr;

                    zr = refR[refIteration] + dzr;
                    zi = refI[refIteration] + dzi;
                    zmag = Math.FusedMultiplyAdd(zr, zr, zi * zi);

                    // Bailout
                    if (zmag > BAILOUT_DOUBLE) break;

                    if (zmag < Math.FusedMultiplyAdd(dzr, dzr, dzi * dzi) || refIteration == escape)
                    {
                        dzr = zr;
                        dzi = zi;
                        refIteration = 0;
                    }
                }
            }

            if (i == maxIterations) return new IterationResult(i, double.NaN, false);
            return new IterationResult(i, zmag);
        }

        public void CalculateReferenceOrbit(BigComplex center, int maxIterations, out ReferenceOrbit orbit)
        {
            ReferenceOrbit o = new ReferenceOrbit(maxIterations);
            BigComplex z = new BigComplex(0, 0);
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

                        z.Real = zr2 - zi2 + center.Real;
                        z.Imaginary = 2 * zrTemp * z.Imaginary + center.Imaginary;

                        if (zr2 + zi2 > BAILOUT_DOUBLE) break;

                        task.Increment(1);
                    }
                });

            o.EscapeIteration = i;
            orbit = o;
        }
        public double GetContinousValue(IterationResult result)
        {
            if (!result.Escaped) return result.Iteration;
            return result.Iteration + 1 - Math.Log(Math.Log(Math.Sqrt(result.MagnitudeSquared))) * ILOG2;
        }
    }
}
