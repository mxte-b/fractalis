using fractalis.Core.Numbers;
using Spectre.Console;

namespace fractalis.Core.Fractals
{
    public static class MandelbrotSightGenerator
    {




        private static readonly BigFloat TOLERANCE = new("1e-200");
        private static readonly int NEWTON_ITERS = 5000;

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
                        (var z, var dz) = RunningDerivative(c, targetPeriod);

                        zoom = dz.MagnitudeSquared / BigFloat.Ten;

                        if (z.MagnitudeSquared < TOLERANCE)
                        {
                            task.Value = NEWTON_ITERS;
                            break;
                        }

                        c -= z / dz;
                        task.Increment(1);
                    }
                });

            return (c, zoom);
        }

        private static (BigComplex, BigComplex) RunningDerivative(BigComplex c, int targetPeriod)
        {
            BigFloat zr = new(0);
            BigFloat zi = new(0);
            BigFloat dzr = new(0);
            BigFloat dzi = new(0);

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
            }

            return (new(zr, zi), new(dzr, dzi));
        }
    }
}
