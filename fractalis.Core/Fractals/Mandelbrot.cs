using fractalis.Core.Numbers;
using Spectre.Console;
using System.Runtime.CompilerServices;

namespace fractalis.Core.Fractals
{
    /// <summary>
    /// Provides iteration methods for the Mandelbrot fractal.
    /// </summary>
    /// <remarks>
    /// Supports standard, high-dynamic-range, and SIMD-accelerated iterations.
    /// Can calculate reference orbits for perturbation techniques used in deep zooms.
    /// </remarks>
    public class Mandelbrot : IPerturbableFractal, ISimdPerturbableFractal
    {
        /// <summary>
        /// Constant for converting logarithms to base 2.
        /// </summary>
        private const double                ILOG2           = 1.4426950408889634;

        /// <summary>
        /// Maximum magnitude before a point is considered escaped in high-precision iterations.
        /// </summary>
        private static readonly FloatExp    BAILOUT         = new(1, 7);

        /// <summary>
        /// Maximum magnitude before a point is considered escaped in double-precision iterations.
        /// </summary>
        private static readonly double      BAILOUT_DOUBLE  = Math.Pow(2, 7);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsInterior(Complex z)
        {
            // Bulb check
            double zr1 = z.Real + 1;
            double zi2 = z.Imaginary * z.Imaginary;
            if (zr1 * zr1 + zi2 <= 0.0625) return true;

            // Cardioid check
            double zr14 = z.Real - 0.25;
            double q = zr14 * zr14 + zi2;
            if (q * (q + zr14) <= 0.25 * zi2) return true;

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vec256d IsInteriorSimd(Vec256d zr, Vec256d zi)
        {
            // Bulb check
            Vec256d one = SimdAgnostic.Create(1);
            Vec256d quarter = SimdAgnostic.Create(0.25);
            Vec256d sixteenth = SimdAgnostic.Create(0.0625);

            Vec256d zr1 = SimdAgnostic.Add(zr, one);
            Vec256d zi2 = SimdAgnostic.Multiply(zi, zi);
            Vec256d bulb = SimdAgnostic.CompareLessThan(SimdAgnostic.MultiplyAdd(zr1, zr1, zi2), sixteenth);

            // Cardioid check
            Vec256d zr14 = SimdAgnostic.Subtract(zr, quarter);
            Vec256d q = SimdAgnostic.MultiplyAdd(zr14, zr14, zi2);
            Vec256d cardioid = SimdAgnostic.CompareLessThan(
                SimdAgnostic.Multiply(q, SimdAgnostic.Add(q, zr14)), 
                SimdAgnostic.Multiply(quarter, zi2));

            return SimdAgnostic.Or(bulb, cardioid);
        }

        #region Iterations
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public IterationResult Iteration(Complex c, int maxIterations)
        {
            if (IsInterior(c)) return new IterationResult(maxIterations, double.NaN);

            Complex z = new(0, 0);
            int i = 0;

            for (; i < maxIterations; i++)
            {
                double zrTemp = z.Real;

                z.Real = z.Real * z.Real - z.Imaginary * z.Imaginary + c.Real;
                z.Imaginary = 2 * zrTemp * z.Imaginary + c.Imaginary;

                if (z.MagnitudeSquared > BAILOUT_DOUBLE) break;
            }

            if (i == maxIterations) return new IterationResult(i, double.NaN);

            return new IterationResult(i, z.MagnitudeSquared);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public IterationResult IterationFloatExpPerturbed(ScaledComplex delta, int maxIterations, in ReferenceOrbit referenceOrbit)
        {
            int i = 0;
            int refIteration = 0;
            ScaledComplex dz = new(0, 0);
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

            if (i == maxIterations) return new IterationResult(i, double.NaN);

            return new IterationResult(i, escapeMag);
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

            if (i == maxIterations) return new IterationResult(i, double.NaN);
            return new IterationResult(i, zmag);
        }
        #endregion

        #region SIMD-accelerated iterations
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe (IterationResult r0, IterationResult r1, IterationResult r2, IterationResult r3) IterationSIMD(Vec256d cr, Vec256d ci, int maxIterations)
        {
            Vec256d one = SimdAgnostic.Create(1.0);
            Vec256d zr = SimdAgnostic.Zero;
            Vec256d zi = SimdAgnostic.Zero;
            Vec256d zr2 = SimdAgnostic.Zero;
            Vec256d zi2 = SimdAgnostic.Zero;
            Vec256d iterations = SimdAgnostic.Zero;
            Vec256d escapeMags = SimdAgnostic.Zero;
            Vec256d outside = SimdAgnostic.AndNot(IsInteriorSimd(cr, ci), SimdAgnostic.AllBitsSet);
            Vec256d bailout = SimdAgnostic.Create(BAILOUT_DOUBLE);
            Vec256d active = outside;

            for (int i = 0; i < maxIterations; i++)
            {
                Vec256d zrzi = SimdAgnostic.Multiply(zr, zi);

                zr = SimdAgnostic.Add(SimdAgnostic.Subtract(zr2, zi2), cr);
                zi = SimdAgnostic.Add(SimdAgnostic.Add(zrzi, zrzi), ci);

                zr2 = SimdAgnostic.Multiply(zr, zr);
                zi2 = SimdAgnostic.Multiply(zi, zi);

                // Bailout if every point escaped
                Vec256d zmag = SimdAgnostic.Add(zr2, zi2);
                Vec256d prevActive = active;

                active = SimdAgnostic.And(SimdAgnostic.CompareLessThan(zmag, bailout), outside);
                escapeMags = SimdAgnostic.BlendVariable(escapeMags, zmag, SimdAgnostic.AndNot(active, prevActive));

                if (SimdAgnostic.TestZ(active, active)) break;

                // Increment iterations
                iterations = SimdAgnostic.Add(iterations, SimdAgnostic.And(active, one));
            }

            double* magnitudeBuffer = stackalloc double[4];
            double* iterationBuffer = stackalloc double[4];
            double* interiorBuffer = stackalloc double[4];

            SimdAgnostic.Store(magnitudeBuffer, escapeMags);
            SimdAgnostic.Store(iterationBuffer, iterations);
            SimdAgnostic.Store(interiorBuffer, outside);

            int i0 = (int)iterationBuffer[0];
            int i1 = (int)iterationBuffer[1];
            int i2 = (int)iterationBuffer[2];
            int i3 = (int)iterationBuffer[3];

            double z0 = magnitudeBuffer[0];
            double z1 = magnitudeBuffer[1];
            double z2 = magnitudeBuffer[2];
            double z3 = magnitudeBuffer[3];

            static IterationResult Make(int i, double z, int m, bool interior) 
                => interior ? new(m, double.NaN) : new(i, i < m ? z : double.NaN);

            return (
                Make(i0, z0, maxIterations, interiorBuffer[0] == 0),
                Make(i1, z1, maxIterations, interiorBuffer[1] == 0),
                Make(i2, z2, maxIterations, interiorBuffer[2] == 0), 
                Make(i3, z3, maxIterations, interiorBuffer[3] == 0));
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe (IterationResult r0, IterationResult r1, IterationResult r2, IterationResult r3) IterationPerturbedSIMD(Vec256d deltaR, Vec256d deltaI, int maxIterations, in ReferenceOrbit referenceOrbit)
        {
            Vec256d dzr = SimdAgnostic.Zero;
            Vec256d dzi = SimdAgnostic.Zero;
            Vec256d iterations = SimdAgnostic.Zero;
            Vec256d escapeMags = SimdAgnostic.Zero;
            Vec256d active = SimdAgnostic.AllBitsSet;
            Vec256d one = SimdAgnostic.Create(1.0);
            Vec256d two = SimdAgnostic.Create(2.0);
            Vec256d bailout = SimdAgnostic.Create(BAILOUT_DOUBLE);

            int escape = referenceOrbit.EscapeIteration - 1;

            // Get pointer to reference orbit, without GC
            fixed (double* refR = referenceOrbit.PointsR)
            fixed (double* refI = referenceOrbit.PointsI)
            {
                int refIteration = 0;

                for (int i = 0; i < maxIterations; i++)
                {
                    // Read out reference point
                    Vec256d zr_ref = SimdAgnostic.Create(refR[refIteration]);
                    Vec256d zi_ref = SimdAgnostic.Create(refI[refIteration]);
                    refIteration++;

                    // 2 * z_ref + delta
                    Vec256d ar = SimdAgnostic.MultiplyAdd(two, zr_ref, dzr);
                    Vec256d ai = SimdAgnostic.MultiplyAdd(two, zi_ref, dzi);

                    // delta = (2 * z_ref * dz) * dz + dz + delta
                    Vec256d newdzr = SimdAgnostic.MultiplyAdd(ar, dzr, SimdAgnostic.MultiplyAddNegated(ai, dzi, deltaR));
                    Vec256d newdzi = SimdAgnostic.MultiplyAdd(ar, dzi, SimdAgnostic.MultiplyAdd(ai, dzr, deltaI));

                    // Only apply it to non-escaped points
                    dzr = SimdAgnostic.BlendVariable(dzr, newdzr, active);
                    dzi = SimdAgnostic.BlendVariable(dzi, newdzi, active);

                    // Calulate next z
                    // z = z_ref + dz
                    Vec256d zr = SimdAgnostic.Add(SimdAgnostic.Create(refR[refIteration]), dzr);
                    Vec256d zi = SimdAgnostic.Add(SimdAgnostic.Create(refI[refIteration]), dzi);
                    Vec256d zmag = SimdAgnostic.MultiplyAdd(zr, zr, SimdAgnostic.Multiply(zi, zi));
                    Vec256d prevActive = active;

                    // Bailout if every point escaped
                    active = SimdAgnostic.And(prevActive, SimdAgnostic.CompareLessThan(zmag, bailout));

                    // Store magnitudes for points that just escaped
                    escapeMags = SimdAgnostic.BlendVariable(escapeMags, zmag, SimdAgnostic.AndNot(active, prevActive));
                    if (SimdAgnostic.TestZ(active, active)) break;

                    // Prevent delta from straying off and causing visual glitches
                    bool needsRebase = false;

                    // For performance, only check glitches every 4th iteration
                    // NEEDS TESTING FOR GLITCHES
                    if ((i & 3) == 0)
                    {
                        Vec256d dzmag = SimdAgnostic.MultiplyAdd(dzr, dzr, SimdAgnostic.Multiply(dzi, dzi));

                        // Only rebase if all lanes are glitched
                        Vec256d cmp = SimdAgnostic.And(
                            SimdAgnostic.Xor(SimdAgnostic.AllBitsSet, SimdAgnostic.CompareLessThan(zmag, dzmag)),
                            active);

                        needsRebase = SimdAgnostic.TestZ(cmp, cmp);
                    }

                    if (needsRebase || refIteration == escape)
                    {
                        dzr = zr; dzi = zi; refIteration = 0;
                    }

                    // Increment iterations
                    iterations = SimdAgnostic.Add(iterations, SimdAgnostic.And(active, one));
                }
            }

            double* magnitudeBuffer = stackalloc double[4];
            double* iterationBuffer = stackalloc double[4];

            SimdAgnostic.Store(magnitudeBuffer, escapeMags);
            SimdAgnostic.Store(iterationBuffer, iterations);

            int i0 = (int)iterationBuffer[0];
            int i1 = (int)iterationBuffer[1];
            int i2 = (int)iterationBuffer[2];
            int i3 = (int)iterationBuffer[3];

            double z0 = magnitudeBuffer[0];
            double z1 = magnitudeBuffer[1];
            double z2 = magnitudeBuffer[2];
            double z3 = magnitudeBuffer[3];

            static IterationResult Make(int i, double z, int m) => new(i, i < m ? z : double.NaN);

            return (Make(i0, z0, maxIterations), Make(i1, z1, maxIterations), Make(i2, z2, maxIterations), Make(i3, z3, maxIterations));
        }
        #endregion

        public void CalculateReferenceOrbit(BigComplex center, int maxIterations, out ReferenceOrbit orbit)
        {
            ReferenceOrbit o = new(maxIterations);
            BigComplex z = new(0, 0);
            int i = 0;

            for (; i < maxIterations; i++)
            {
                Complex zc = z.ToComplex();
                o.PointsR[i] = zc.Real;
                o.PointsI[i] = zc.Imaginary;
                o.ScaledPoints[i] = z.ToScaledComplex();

                using BigFloat zrTemp = z.Real;
                using BigFloat zr2 = z.Real * z.Real;
                using BigFloat zi2 = z.Imaginary * z.Imaginary;

                z.Real = zr2 - zi2 + center.Real;
                z.Imaginary = 2 * zrTemp * z.Imaginary + center.Imaginary;

                if (zr2 + zi2 > BAILOUT_DOUBLE) break;
            }

            o.EscapeIteration = i;
            orbit = o;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetContinousValue(IterationResult result)
        {
            if (!result.Escaped) return result.Iteration;
            return result.Iteration + 1 - Math.Log(Math.Log(result.MagnitudeSquared) * 0.5) * ILOG2;
        }
    }
}
