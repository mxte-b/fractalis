using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
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

    internal struct ReferenceOrbit
    {
        public Complex[] Points;
        public int EscapeIteration;

        public ReferenceOrbit(int maxIterations)
        {
            Points = new Complex[maxIterations];
            EscapeIteration = 0;
        }
    }

    public class MandelbrotRenderer
    {

        public int Iterations, Width, Height;
        public double Zoom { get; set; }
        public Complex Center { get; set; }
        public BigFixed ZoomHigh { get; set; }
        public BigComplex CenterHigh { get; set; }
        public static ColorPalette ColorPalette { get; set; }
        
        private static double ILOG2 = 1 / Math.Log(2);

        private double PixelSpacing {
            get
            {
                return 1 / (Width * Zoom);
            }
        }

        public RenderMode RenderMode
        {
            get
            {
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

        private ReferenceOrbit ReferenceOrbit;
        private double Iteration(Complex c)
        {
            Complex z = new Complex(0, 0);
            int i = 0;
            int max = Iterations;

            for (; i < max; i++)
            {
                double zrTemp = z.Real;

                z.Real = z.Real * z.Real - z.Imaginary * z.Imaginary + c.Real;
                z.Imaginary = 2 * zrTemp * z.Imaginary + c.Imaginary;

                if (z.MagnitudeSquared > 100) break;
            }

            if (i == max) return max;

            return i + 1 - Math.Log(Math.Log(z.Magnitude)) * ILOG2;
        }

        private double IterationPerturbed(Complex dc)
        {
            int i = 0;
            int refIteration = 0;
            int max = Iterations;
            Complex dz = new Complex(0, 0);
            Complex lastZ = new Complex(0, 0);

            for (; i < max; i++)
            {
                dz = (2 * ReferenceOrbit.Points[refIteration++] + dz) * dz + dc;

                Complex z = ReferenceOrbit.Points[refIteration] + dz;

                // Bailout
                if (z.MagnitudeSquared > 100)
                {
                    lastZ = z;
                    break;
                }

                // Prevent delta from straying off and causing visual glitches
                if (z.MagnitudeSquared < dz.MagnitudeSquared || refIteration == ReferenceOrbit.EscapeIteration - 1)
                {
                    dz = z;
                    refIteration = 0;
                }
            }

            if (i == max) return max;
            return i + 1 - Math.Log(Math.Log(lastZ.Magnitude)) * ILOG2;
        }

        private void CalculateCenterOrbit()
        {
            BigComplex z = new BigComplex(0, 0);
            int i = 0;

            for (; i < Iterations; i++)
            {
                ReferenceOrbit.Points[i] = z.ToComplex();

                BigFixed zrTemp = z.Real;

                z.Real = z.Real * z.Real - z.Imaginary * z.Imaginary + CenterHigh.Real;
                z.Imaginary = 2 * zrTemp * z.Imaginary + CenterHigh.Imaginary;

                if (z.MagnitudeSquared > 100) break;
            }

            ReferenceOrbit.EscapeIteration = i;
        }

        private Rgb24 ComputePixel(int x, int y)
        {
            double ndcX = (double)x / Width - 0.5;
            double ndcY = -((double)y / Height - 0.5);
            ndcX *= (double)Width / Height;

            double iterations;

            if (RenderMode == RenderMode.HighPrecision)
            {

                if (x == Width / 2 && y == Height / 2)
                {
                    iterations = ReferenceOrbit.EscapeIteration;
                }
                else
                { 
                    BigComplex dc = new BigComplex(ndcX / ZoomHigh, ndcY / ZoomHigh);
                    iterations = IterationPerturbed(dc.ToComplex());
                }
            }
            else
            {
                Complex c = new Complex(ndcX / Zoom + Center.Real, ndcY / Zoom + Center.Imaginary);
                iterations = Iteration(c);
            }

            return ColorPalette.Sample(iterations).ToPixel<Rgb24>();
        }

        public Image<Rgb24> Render()
        {
            Image<Rgb24> image = new Image<Rgb24>(Width, Height);
            int yCompleted = 0;

            Console.WriteLine($"Rendering with: {RenderMode}");

            // High Precision - using Perturbation Theory
            if (RenderMode == RenderMode.HighPrecision)
            {
                CalculateCenterOrbit();
            }

            // Main render loop
            Parallel.For(0, Height, y => {
                for (int x = 0; x < Width; x++)
                {

                    image[x, y] = ComputePixel(x, y);
                }

                Interlocked.Increment(ref yCompleted);

                if (y % Math.Round(Height * 0.005) == 0)
                {
                    Console.WriteLine($"{(double)yCompleted / Height:p}");
                }
            });

            return image;
        }

        public MandelbrotRenderer(int i, int w, int h, BigFixed z, BigComplex c)
        {
            Iterations = i;
            Width = w;
            Height = h;

            ZoomHigh = z;
            CenterHigh = c;

            Zoom = (double)z;
            Center = c.ToComplex();

            ReferenceOrbit = new ReferenceOrbit(i);
        }
    }
}
