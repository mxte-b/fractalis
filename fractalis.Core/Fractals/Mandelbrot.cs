using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Fractals
{
    public class Mandelbrot : IPerturbableFractal
    {
        private const double ILOG2 = 1.4426950408889634;

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

            return new IterationResult(i, z.Magnitude);
        }
        public IterationResult IterationPerturbed(Complex delta, int maxIterations, in ReferenceOrbit referenceOrbit)
        {
            int i = 0;
            int refIteration = 0;
            Complex dz = new Complex(0, 0);
            Complex lastZ = new Complex(0, 0);

            for (; i < maxIterations; i++)
            {
                dz = (2 * referenceOrbit.Points[refIteration++] + dz) * dz + delta;

                Complex z = referenceOrbit.Points[refIteration] + dz;

                // Bailout
                if (z.MagnitudeSquared > 100)
                {
                    lastZ = z;
                    break;
                }

                // Prevent delta from straying off and causing visual glitches
                if (z.MagnitudeSquared < dz.MagnitudeSquared || refIteration == referenceOrbit.EscapeIteration - 1)
                {
                    dz = z;
                    refIteration = 0;
                }
            }

            if (i == maxIterations) return new IterationResult(i, double.NaN, false);

            return new IterationResult(i, lastZ.Magnitude);
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
                        o.Points[i] = z.ToComplex();
                        BigFixed zrTemp = z.Real;
                        z.Real = z.Real * z.Real - z.Imaginary * z.Imaginary + center.Real;
                        z.Imaginary = 2 * zrTemp * z.Imaginary + center.Imaginary;
                        if (z.MagnitudeSquared > 100) break;
                        task.Increment(1);
                    }
                });

            o.EscapeIteration = i;
            orbit = o;
        }
        public double GetContinousValue(IterationResult result)
        {
            if (!result.Escaped) return result.Iteration;
            return result.Iteration + 1 - Math.Log(Math.Log(result.Magnitude)) * ILOG2;
        }
    }
}
