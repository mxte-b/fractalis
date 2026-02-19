using fractalis;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotArbitraryPrecision
{
    public enum RenderMode
    {
        Default,
        HighPrecision
    }

    internal class MandelbrotRenderer
    {
        public int Iterations, Width, Height;

        public RenderMode RenderMode;

        public double Zoom { get; set; }
        public Complex Center { get; set; }

        public BigFixed ZoomHigh { get; set; }
        public BigComplex CenterHigh { get; set; }

        private BigComplex[] ReferencePoints { get; set; }

        private int IterationHigh(BigComplex c)
        {
            BigComplex z = new BigComplex(0, 0);
            int i = 0;

            for (; i < Iterations; i++)
            {
                BigFixed zrTemp = z.Real;

                z.Real = z.Real * z.Real - z.Imaginary * z.Imaginary + c.Real;
                z.Imaginary = 2 * zrTemp * z.Imaginary + c.Imaginary;

                if (z.MagnitudeSquared > 100) break;
            }

            return i;
        }

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

        private Rgb24 ComputePixel(int x, int y)
        {
            double ndcX = (double)x / Width - 0.5;
            double ndcY = (double)y / Height - 0.5;
            ndcX *= (double)Width / Height;

            int iterations;

            if (RenderMode == RenderMode.HighPrecision)
            {
                BigFixed pixelX = ndcX / ZoomHigh;
                BigFixed pixelY = ndcY / ZoomHigh;

                BigComplex c = new BigComplex(
                    CenterHigh.Real + pixelX,
                    CenterHigh.Imaginary + pixelY
                );

                iterations = IterationHigh(c);
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

            Parallel.For(0, Height, y =>
            {
                for (int x = 0; x < Width; x++)
                {
                    image[x, y] = ComputePixel(x, y);
                }

                yCompleted++;
                Console.WriteLine($"{((double)yCompleted / Height):p}");
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
