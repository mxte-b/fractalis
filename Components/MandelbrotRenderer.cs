using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Components
{
    public enum RenderMode
    {
        Default,
        HighPrecision
    }

    internal struct ReferenceOrbit
    {
        public List<Complex> Points;
        public int EscapeIteration;

        public ReferenceOrbit()
        {
            Points = new List<Complex>();
            EscapeIteration = 0;
        }
    }

    internal class MandelbrotRenderer
    {
        public int Iterations, Width, Height;
        public RenderMode RenderMode;
        public double Zoom { get; set; }
        public Complex Center { get; set; }
        public BigFixed ZoomHigh { get; set; }
        public BigComplex CenterHigh { get; set; }
        private ReferenceOrbit ReferenceOrbit = new ReferenceOrbit();

        private int Iteration(Complex c)
        {
            Complex z = new Complex(0, 0);
            int i = 0;

            for (; i < Iterations; i++)
            {
                double zrTemp = z.Real;

                z.Real = z.Real * z.Real - z.Imaginary * z.Imaginary + c.Real;
                z.Imaginary = 2 * zrTemp * z.Imaginary + c.Imaginary;

                if (z.MagnitudeSquared > 100) break;
            }

            return i;
        }

        private int IterationPerturbed(Complex dc)
        {
            int i = 0;
            int refIteration = 0;
            Complex dz = new Complex(0, 0);

            for (; i < Iterations; i++)
            {
                dz = (2 * ReferenceOrbit.Points[refIteration++] + dz) * dz + dc;

                Complex z = ReferenceOrbit.Points[refIteration] + dz;

                // Bailout
                if (z.MagnitudeSquared > 100) break;

                // Prevent delta from straying off and causing visual glitches
                if (z.MagnitudeSquared < dz.MagnitudeSquared || refIteration == ReferenceOrbit.EscapeIteration - 1)
                {
                    dz = z;
                    refIteration = 0;
                }
            }

            return i;
        }

        private void CalculateCenterOrbit()
        {
            BigComplex z = new BigComplex(0, 0);
            int i = 0;

            for (; i < Iterations; i++)
            {
                ReferenceOrbit.Points.Add(z.ToComplex());

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

            int iterations;

            if (RenderMode == RenderMode.HighPrecision)
            {

                if (x == Width / 2 && y == Height / 2)
                {
                    iterations = ReferenceOrbit.EscapeIteration;
                }
                else
                {
                    BigFixed pixelX = ndcX / ZoomHigh;
                    BigFixed pixelY = ndcY / ZoomHigh;

                    BigComplex dc = new BigComplex(pixelX, pixelY);

                    iterations = IterationPerturbed(dc.ToComplex());
                }
            }
            else
            {
                Complex c = new Complex(ndcX / Zoom + Center.Real, ndcY / Zoom + Center.Imaginary);
                iterations = Iteration(c);
            }

            byte norm = (byte)Math.Round(iterations * 255d / Iterations);

            if (norm == 255) norm = 0;

            return new Rgb24(norm, norm, norm);
        }

        public Image<Rgb24> Render()
        {
            Image<Rgb24> image = new Image<Rgb24>(Width, Height);
            int yCompleted = 0;

            // High Precision - using Perturbation Theory
            if (RenderMode == RenderMode.HighPrecision)
            {
                CalculateCenterOrbit();
            }

            // Main render loop
            Parallel.For(0, Height, y =>
            {
                for (int x = 0; x < Width; x++)
                {
                    image[x, y] = ComputePixel(x, y);
                }

                Interlocked.Increment(ref yCompleted);
                Console.WriteLine($"{(double)yCompleted / Height:p}");
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
            RenderMode = RenderMode.HighPrecision;
        }

        public MandelbrotRenderer(int i, int w, int h, double z, Complex c)
        {
            Iterations = i;
            Width = w;
            Height = h;
            Zoom = z;
            Center = c;
            RenderMode = RenderMode.Default;
        }
    }
}
