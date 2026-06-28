using fractalis.Core.Numbers;
using Spectre.Console;

namespace fractalis.Core.Fractals
{
    public static class MandelbrotSightGenerator
    {
        private static readonly BigFloat TOLERANCE = new("1e-200");
        private static readonly int NEWTON_ITERS = 10000;

        //public static (BigComplex, BigFloat) FindMisiurewicz(BigComplex near, int preperiod, int targetPeriod)
        //{

        //}

        /// <summary>
        /// Generates a coordinate of a minibrot from an initial position guess and a given period value.
        /// </summary>
        /// <param name="near">The initial guess for the Minibrot's position.</param>
        /// <param name="targetPeriod">The target period value of the Minibrot.</param>
        /// <returns>The approximate position and approximate zoom depth of the Minibrot.</returns>
        public static (BigComplex, BigFloat) FindMinibrot(BigComplex near, int targetPeriod, bool showProgress = true)
        {
            BigComplex c = near;
            BigFloat zoom = BigFloat.One;

            if (!showProgress)
            {
                for (int i = 0; i < NEWTON_ITERS; i++)
                {
                    (var z, var dz, var isPure) = RunningDerivative(c, targetPeriod);

                    zoom = dz.MagnitudeSquared / BigFloat.Ten;

                    if (c.MagnitudeSquared < new BigFloat(1e-5))
                    {
                        Console.WriteLine("Converged to main bulb.");
                        break;
                    }

                    if (z.MagnitudeSquared < TOLERANCE && isPure)
                    {
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
                }

                Console.WriteLine($"{targetPeriod} - Distance from original guess: {(c - near).Magnitude}");
                return (c, zoom);
            }

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

        #region Running derivatives
        private static (BigComplex z, BigComplex dz, bool isPureOrbit) RunningDerivative(BigComplex c, int targetPeriod)
        {
            BigFloat zr  = new(0);
            BigFloat zi  = new(0);
            BigFloat dzr = new(0);
            BigFloat dzi = new(0);

            // Loop variables
            BigFloat zrTemp  = new(0);
            BigFloat dzrTemp = new(0);
            BigFloat zr2 = new(0);
            BigFloat zi2 = new(0);
            BigFloat zmag = new(0);

            // Temporary variables
            BigFloat t0 = new(0);
            BigFloat t1 = new(0);

            bool isPurePeriod = true;

            for (int i = 0; i < targetPeriod; i++)
            {
                zrTemp.SetFrom(zr);
                dzrTemp.SetFrom(dzr);

                // zr2 = zr * zr;
                zr2.SetFrom(zr);
                zr2.MultiplyInPlace(zr);

                // zi2 = zi * zi;
                zi2.SetFrom(zi);
                zi2.MultiplyInPlace(zi);

                // zmag = zr2 + zi2;
                zmag.SetFrom(zr2);
                zmag.AddInPlace(zi2);

                if (zmag > 100) break;

                // dzr = 2 * (zr * dzr - zi * dzi) + BigFloat.One;
                t0.SetFrom(zr);
                t0.MultiplyInPlace(dzr);

                t1.SetFrom(zi);
                t1.MultiplyInPlace(dzi);

                dzr.SetFrom(t0);
                dzr.SubtractInPlace(t1);
                dzr.MultiplyInPlace(2);
                dzr.AddInPlace(BigFloat.One);

                // dzi = 2 * (zr * dzi + zi * dzrTemp);
                t0.SetFrom(zr);
                t0.MultiplyInPlace(dzi);

                t1.SetFrom(zi);
                t1.MultiplyInPlace(dzrTemp);

                dzi.SetFrom(t0);
                dzi.AddInPlace(t1);
                dzi.MultiplyInPlace(2);

                // zr = zr2 - zi2 + c.Real;
                zr.SetFrom(zr2);
                zr.SubtractInPlace(zi2);
                zr.AddInPlace(c.Real);

                // zi = 2 * zrTemp * zi + c.Imaginary;
                zi.MultiplyInPlace(zrTemp);
                zi.MultiplyInPlace(2);
                zi.AddInPlace(c.Imaginary);

                // If at any point we get below tolerance value, that means that the
                // current orbit is not of the target period
                if (i > 0 && i <= targetPeriod / 2 && zmag < TOLERANCE) isPurePeriod = false;
            }

            return (new(zr.Clone(), zi.Clone()), new(dzr.Clone(), dzi.Clone()), isPurePeriod);
        }

        private static (BigComplex value, BigComplex derivative, bool isPureOrbit) MisiurewiczRunningDerivative(BigComplex c, int preperiod, int targetPeriod)
        {
            BigFloat zr = new(0);
            BigFloat zi = new(0);
            BigFloat dzr = new(0);
            BigFloat dzi = new(0);

            // Values at preperiod
            BigFloat zrK = new(0);
            BigFloat ziK = new(0);
            BigFloat dzrK = new(0);
            BigFloat dziK = new(0);

            // Loop variables
            BigFloat zrTemp = new(0);
            BigFloat dzrTemp = new(0);
            BigFloat zr2 = new(0);
            BigFloat zi2 = new(0);
            BigFloat zmag = new(0);

            // Temporary variables
            BigFloat t0 = new(0);
            BigFloat t1 = new(0);

            bool isPurePeriod = true;

            for (int i = 0; i < targetPeriod + preperiod; i++)
            {
                zrTemp.SetFrom(zr);
                dzrTemp.SetFrom(dzr);

                // zr2 = zr * zr;
                zr2.SetFrom(zr);
                zr2.MultiplyInPlace(zr);

                // zi2 = zi * zi;
                zi2.SetFrom(zi);
                zi2.MultiplyInPlace(zi);

                // zmag = zr2 + zi2;
                zmag.SetFrom(zr2);
                zmag.AddInPlace(zi2);

                if (zmag > 100) break;

                // dzr = 2 * (zr * dzr - zi * dzi) + BigFloat.One;
                t0.SetFrom(zr);
                t0.MultiplyInPlace(dzr);

                t1.SetFrom(zi);
                t1.MultiplyInPlace(dzi);

                dzr.SetFrom(t0);
                dzr.SubtractInPlace(t1);
                dzr.MultiplyInPlace(2);
                dzr.AddInPlace(BigFloat.One);

                // dzi = 2 * (zr * dzi + zi * dzrTemp);
                t0.SetFrom(zr);
                t0.MultiplyInPlace(dzi);

                t1.SetFrom(zi);
                t1.MultiplyInPlace(dzrTemp);

                dzi.SetFrom(t0);
                dzi.AddInPlace(t1);
                dzi.MultiplyInPlace(2);

                // zr = zr2 - zi2 + c.Real;
                zr.SetFrom(zr2);
                zr.SubtractInPlace(zi2);
                zr.AddInPlace(c.Real);

                // zi = 2 * zrTemp * zi + c.Imaginary;
                zi.MultiplyInPlace(zrTemp);
                zi.MultiplyInPlace(2);
                zi.AddInPlace(c.Imaginary);

                // Save values at preperiod
                if (i + 1 == preperiod)
                {
                    zrK.SetFrom(zr);
                    ziK.SetFrom(zi);
                    dzrK.SetFrom(dzr);
                    dziK.SetFrom(dzi);
                }

                // If at any point we get below tolerance value, that means that the
                // current orbit is not of the target period
                if (i > preperiod && i <= targetPeriod + preperiod / 2 && zmag < TOLERANCE) isPurePeriod = false;
            }

            // Calculate values for Newton iteration
            zr.SubtractInPlace(zrK);
            zi.SubtractInPlace(ziK);
            dzr.SubtractInPlace(dzrK);
            dzi.SubtractInPlace(dziK);

            return (new(zr.Clone(), zi.Clone()), new(dzr.Clone(), dzi.Clone()), isPurePeriod);
        }
        #endregion
    }
}
