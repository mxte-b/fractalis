using fractalis.Core.Numbers;
using Spectre.Console;

namespace fractalis.Core.Fractals
{
    public static class MandelbrotSightGenerator
    {
        private static readonly BigFloat TOLERANCE = new("1e-200");
        private static readonly int NEWTON_ITERS = 10000;

        /// <summary>
        /// Generates a coordinate of a minibrot from an initial position guess and a given period value.
        /// </summary>
        /// <param name="near">The initial guess for the Minibrot's position.</param>
        /// <param name="targetPeriod">The target period value of the Minibrot.</param>
        /// <returns>The approximate position and approximate zoom depth of the Minibrot.</returns>
        public static (BigComplex, BigFloat) FindMinibrot(BigComplex near, int targetPeriod)
        {
            BigComplex c = near;
            BigFloat zoom = BigFloat.One;

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
                    var task = ctx.AddTask("<#> Generating location:".PadRight(31), maxValue: NEWTON_ITERS);

                    for (int i = 0; i < NEWTON_ITERS; i++)
                    {
                        (var z, var dz, var isPure) = RunningDerivative(c, targetPeriod);

                        zoom = dz.MagnitudeSquared / BigFloat.Ten;

                        if (z.MagnitudeSquared < TOLERANCE && isPure)
                        {
                            task.Value = NEWTON_ITERS;
                            break;
                        }

                        if (isPure)
                        {
                            c -= z / dz;
                        }
                        // To prevent Newton from getting stuck in attractors that correspond to
                        // an orbit value dividing targetPeriod, we move c towards the initial guess
                        else
                        {
                            c = (c + near) / new BigComplex(2, 0);
                        }

                        task.Increment(1);
                    }
                });

            Console.WriteLine($"Distance from original guess: {(c - near).Magnitude}");
            return (c, zoom);
        }

        private static (BigComplex, BigComplex, bool) RunningDerivative(BigComplex c, int targetPeriod)
        {
            BigFloat zr = new(0);
            BigFloat zi = new(0);
            BigFloat dzr = new(0);
            BigFloat dzi = new(0);
            bool isPurePeriod = true;

            for (int i = 0; i < targetPeriod; i++)
            {
                BigFloat zrTemp = zr;
                BigFloat dzrTemp = dzr;

                BigFloat zr2 = zr * zr;
                BigFloat zi2 = zi * zi;

                dzr = 2 * (zr * dzr - zi * dzi) + BigFloat.One;
                dzi = 2 * (zr * dzi + zi * dzrTemp);

                zr = zr2 - zi2 + c.Real;
                zi = 2 * zrTemp * zi + c.Imaginary;

                if (zr2 + zi2 > 100) break;

                // If at any point we get below tolerance value, that means that the
                // current orbit is not of the target period
                if (i > 0 && i <= targetPeriod / 2 && zr2 + zi2 < TOLERANCE) isPurePeriod = false;
            }

            return (new(zr, zi), new(dzr, dzi), isPurePeriod);
        }
    }
}
