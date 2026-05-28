using fractalis.Core.Fractals;
using fractalis.Core.Numbers;
using SixLabors.ImageSharp.PixelFormats;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core
{
    public partial class FractalRenderer
    {
        #region Row rendering
        private void RenderRowScalar(Rgb24[] colorBuffer, int y)
        {
            double ndcY = NdcY(y);
            int rowOffset = y * Width;

            for (int x = 0; x < Width; x++)
            {
                double ndcX = NdcX(x);
                IterationResult result = Fractal.Iteration(PixelCoordinates(ndcX, ndcY), Iterations);
                colorBuffer[rowOffset + x] = Sample(result);
            }
        }

        private void RenderRowSimd(Rgb24[] colorBuffer, int y, ISimdFractal simd)
        {
            double ndcY = NdcY(y);

            Vec256d ci = SimdAgnostic.Create(ndcY * _pixelSpacingDouble + _centerDouble.Imaginary);
            Vec256d stepX = SimdAgnostic.Create(2.0 * _pixelSpacingDouble / Height);
            Vec256d centerReal = SimdAgnostic.Create(-_aspectRatio * _pixelSpacingDouble + _centerDouble.Real);

            int rowOffset = y * Width;

            for (int x = 0; x < Width; x += 4)
            {
                var cr = SimdAgnostic.MultiplyAdd(
                    SimdAgnostic.Create(x, x + 1.0, x + 2.0, x + 3.0),
                    stepX,
                    centerReal);

                var (r0, r1, r2, r3) = simd.IterationSIMD(cr, ci, Iterations);

                switch (Width - x)
                {
                    default: colorBuffer[rowOffset + x + 3] = Sample(r3); goto case 3;
                    case 3: colorBuffer[rowOffset + x + 2]  = Sample(r2); goto case 2;
                    case 2: colorBuffer[rowOffset + x + 1]  = Sample(r1); goto case 1;
                    case 1: colorBuffer[rowOffset + x]      = Sample(r0); break;
                }
            }
        }

        private void RenderRowPerturbed(Rgb24[] colorBuffer, int y, IPerturbableFractal p)
        {
            double ndcY = NdcY(y);
            int rowOffset = y * Width;

            for (int x = 0; x < Width; x++)
            {
                double ndcX = NdcX(x);
                IterationResult result = p.IterationPerturbed(ndcX * _pixelSpacingDouble, ndcY * _pixelSpacingDouble, Iterations, in _referenceOrbit);
                colorBuffer[rowOffset + x] = Sample(result);
            }
        }

        private void RenderRowFloatExp(Rgb24[] colorBuffer, int y, IPerturbableFractal p)
        {
            double ndcY = NdcY(y);
            int rowOffset = y * Width;

            for (int x = 0; x < Width; x++)
            {
                double ndcX = NdcX(x);
                ScaledComplex delta = new(ndcX * _pixelSpacing, ndcY * _pixelSpacing);
                IterationResult result = p.IterationFloatExp(delta, Iterations, in _referenceOrbit);
                colorBuffer[rowOffset + x] = Sample(result);
            }
        }

        private void RenderRowSimdPerturbed(Rgb24[] colorBuffer, int y, ISimdPerturbableFractal simd)
        {
            var ndcY = SimdAgnostic.Create(NdcY(y) * _pixelSpacingDouble);
            double ndcStepX = 2.0 / Height;
            int rowOffset = y * Width;

            for (int x = 0; x < Width; x += 4)
            {
                var ndcX = SimdAgnostic.MultiplyAdd(
                    SimdAgnostic.Create(x, x + 1.0, x + 2.0, x + 3.0),
                    SimdAgnostic.Create(ndcStepX * _pixelSpacingDouble),
                    SimdAgnostic.Create(-_aspectRatio * _pixelSpacingDouble));

                var (r0, r1, r2, r3) = simd.IterationPerturbedSIMD(ndcX, ndcY, Iterations, in _referenceOrbit);

                switch (Width - x)
                {
                    default: colorBuffer[rowOffset + x + 3] = Sample(r3); goto case 3;
                    case 3: colorBuffer[rowOffset + x + 2] = Sample(r2); goto case 2;
                    case 2: colorBuffer[rowOffset + x + 1] = Sample(r1); goto case 1;
                    case 1: colorBuffer[rowOffset + x] = Sample(r0); break;
                }
            }
        }
        #endregion

        #region Anti-aliasing

        // AA offsets derived from the 7th roots of unity
        private static readonly double[] AA_OFFSETS_X = [1, -0.22252093, -0.90096887,  0.62348980, -0.90096887, -0.22252093,  0.62348980, 0];
        private static readonly double[] AA_OFFSETS_Y = [0,  0.97492791, -0.43388374,  0.78183148,  0.43388374, -0.97492791, -0.78183148, 0];
        private const double AA_SCALE = 0.5;

        private void AAPassScalar(Rgb24[] colorBuffer, Rgb24[] aaBuffer, int y)
        {
            double ndcY = NdcY(y);
            double stepX = 2.0 * _pixelSpacingDouble / Height;
            double halfStepX = stepX * AA_SCALE;
            double halfStepY = stepX * AA_SCALE * 0.5;

            int rowOffset = y * Width;

            for (int x = 0; x < Width; x++)
            {
                if (!NeedsAA(colorBuffer, x, y)) continue;

                Complex c = PixelCoordinates(NdcX(x), ndcY);
                Rgb24 center = colorBuffer[rowOffset + x];

                int r = center.R * 3, 
                    g = center.G * 3, 
                    b = center.B * 3;

                int totalWeight = 3;

                for (int i = 0; i < _aaSamples - 1; i++)
                {
                    Rgb24 color = Sample(
                        Fractal.Iteration(c + new Complex(AA_OFFSETS_X[i] * halfStepX, AA_OFFSETS_Y[i] * halfStepY),Iterations)
                    );

                    r += color.R; 
                    g += color.G; 
                    b += color.B;

                    totalWeight++;
                }

                aaBuffer[rowOffset + x] = new Rgb24((byte)(r / totalWeight), (byte)(g / totalWeight), (byte)(b / totalWeight));
            }
        }

        private void AAPassSimd(Rgb24[] colorBuffer, Rgb24[] aaBuffer, int y, ISimdFractal s)
        {
            double ndcY = NdcY(y);
            double stepX = 2.0 * _pixelSpacingDouble / Height;
            double halfStepX = stepX * AA_SCALE;
            double halfStepY = stepX * AA_SCALE * 0.5;

            Vec256d halfStepXVec = SimdAgnostic.Create(halfStepX);
            Vec256d halfStepYVec = SimdAgnostic.Create(halfStepY);
            Vec256d centerReal = SimdAgnostic.Create(-_aspectRatio * _pixelSpacingDouble + _centerDouble.Real);
            Vec256d centerImag = SimdAgnostic.Create(ndcY * _pixelSpacingDouble + _centerDouble.Imaginary);

            int rowOffset = y * Width;

            for (int x = 0; x < Width; x++)
            {
                if (!NeedsAA(colorBuffer, x, y)) continue;

                Rgb24 center = colorBuffer[rowOffset + x];
                Vec256d pixelCr = SimdAgnostic.Add(SimdAgnostic.Create(x * stepX), centerReal);

                int r = center.R * 3,
                    g = center.G * 3,
                    b = center.B * 3;

                int totalWeight = 3;

                for (int i = 0; i < _aaSamples / 4; i++)
                {
                    int lutOffset = i * 4;

                    var cr = SimdAgnostic.MultiplyAdd(
                        SimdAgnostic.Create(AA_OFFSETS_X[lutOffset], AA_OFFSETS_X[lutOffset + 1], AA_OFFSETS_X[lutOffset + 2], AA_OFFSETS_X[lutOffset + 3]),
                        halfStepXVec,
                        pixelCr);

                    var ci = SimdAgnostic.MultiplyAdd(
                        SimdAgnostic.Create(AA_OFFSETS_Y[lutOffset], AA_OFFSETS_Y[lutOffset + 1], AA_OFFSETS_Y[lutOffset + 2], AA_OFFSETS_Y[lutOffset + 3]),
                        halfStepYVec,
                        centerImag);

                    var (r0, r1, r2, r3) = s.IterationSIMD(cr, ci, Iterations);

                    Rgb24 c0 = Sample(r0),
                          c1 = Sample(r1),
                          c2 = Sample(r2),
                          c3 = Sample(r3);

                    r += c0.R + c1.R + c2.R + c3.R;
                    g += c0.G + c1.G + c2.G + c3.G;
                    b += c0.B + c1.B + c2.B + c3.B;

                    totalWeight += 4;
                }

                aaBuffer[rowOffset + x] = new Rgb24((byte)(r / totalWeight), (byte)(g / totalWeight), (byte)(b / totalWeight));
            }
        }

        private void AAPassPerturbed(Rgb24[] colorBuffer, Rgb24[] aaBuffer, int y, IPerturbableFractal p)
        {
            double ndcY = NdcY(y) * _pixelSpacingDouble;
            double stepX = 2.0 * _pixelSpacingDouble / Height;
            double ndcXStart = -_aspectRatio * _pixelSpacingDouble;
            double halfStepX = stepX * AA_SCALE;
            double halfStepY = stepX * AA_SCALE * 0.5;

            int rowOffset = y * Width;

            for (int x = 0; x < Width; x++)
            {
                if (!NeedsAA(colorBuffer, x, y)) continue;

                double ndcX = x * stepX + ndcXStart;

                Rgb24 center = colorBuffer[rowOffset + x];

                int r = center.R * 3,
                    g = center.G * 3,
                    b = center.B * 3;

                int totalWeight = 3;

                for (int i = 0; i < _aaSamples - 1; i++)
                {
                    double dr = ndcX + AA_OFFSETS_X[i] * halfStepX;
                    double di = ndcY + AA_OFFSETS_Y[i] * halfStepY;

                    Rgb24 color = Sample(
                        p.IterationPerturbed(dr, di, Iterations, in _referenceOrbit)
                    );

                    r += color.R;
                    g += color.G;
                    b += color.B;

                    totalWeight++;
                }

                aaBuffer[rowOffset + x] = new Rgb24((byte)(r / totalWeight), (byte)(g / totalWeight), (byte)(b / totalWeight));
            }
        }

        private void AAPassFloatExp(Rgb24[] colorBuffer, Rgb24[] aaBuffer, int y, IPerturbableFractal p)
        {
            FloatExp ndcY = NdcY(y) * _pixelSpacing;
            FloatExp halfStepX = _pixelSpacing * (2.0 * AA_SCALE / Height);
            FloatExp halfStepY = _pixelSpacing * (AA_SCALE / Height);

            int rowOffset = y * Width;

            for (int x = 0; x < Width; x++)
            {
                if (!NeedsAA(colorBuffer, x, y)) continue;

                FloatExp ndcX = NdcX(x) * _pixelSpacing;

                Rgb24 center = colorBuffer[rowOffset + x];

                int r = center.R * 3,
                    g = center.G * 3,
                    b = center.B * 3;

                int totalWeight = 3;

                for (int i = 0; i < _aaSamples - 1; i++)
                {
                    ScaledComplex delta = new(
                        ndcX + (FloatExp)AA_OFFSETS_X[i] * halfStepX,
                        ndcY + (FloatExp)AA_OFFSETS_Y[i] * halfStepY);

                    Rgb24 color = Sample(
                       p.IterationFloatExp(delta, Iterations, in _referenceOrbit)
                    );

                    r += color.R;
                    g += color.G;
                    b += color.B;

                    totalWeight++;
                }

                aaBuffer[rowOffset + x] = new Rgb24((byte)(r / totalWeight), (byte)(g / totalWeight), (byte)(b / totalWeight));
            }
        }

        private void AAPassSimdPerturbed(Rgb24[] colorBuffer, Rgb24[] aaBuffer, int y, ISimdPerturbableFractal s)
        {
            Vec256d ndcY = SimdAgnostic.Create(NdcY(y) * _pixelSpacingDouble);
            Vec256d ndcXStart = SimdAgnostic.Create(-_aspectRatio * _pixelSpacingDouble);

            Vec256d stepX = SimdAgnostic.Create(2.0 * _pixelSpacingDouble / Height);
            Vec256d aaStepXVec = SimdAgnostic.Multiply(stepX, SimdAgnostic.Create(AA_SCALE));
            Vec256d aaStepYVec = SimdAgnostic.Multiply(stepX, SimdAgnostic.Create(AA_SCALE * 0.5));

            int rowOffset = y * Width;

            for (int x = 0; x < Width; x++)
            {
                if (!NeedsAA(colorBuffer, x, y)) continue;

                Vec256d ndcX = SimdAgnostic.MultiplyAdd(SimdAgnostic.Create(x), stepX, ndcXStart);

                Rgb24 center = colorBuffer[rowOffset + x];

                int r = center.R * 3,
                    g = center.G * 3,
                    b = center.B * 3;

                int totalWeight = 3;

                for (int i = 0; i < _aaSamples / 4; i++)
                {
                    int lutOffset = i * 4;

                    var dr = SimdAgnostic.MultiplyAdd(
                        SimdAgnostic.Create(AA_OFFSETS_X[lutOffset], AA_OFFSETS_X[lutOffset + 1], AA_OFFSETS_X[lutOffset + 2], AA_OFFSETS_X[lutOffset + 3]),
                        aaStepXVec,
                        ndcX);

                    var di = SimdAgnostic.MultiplyAdd(
                        SimdAgnostic.Create(AA_OFFSETS_Y[lutOffset], AA_OFFSETS_Y[lutOffset + 1], AA_OFFSETS_Y[lutOffset + 2], AA_OFFSETS_Y[lutOffset + 3]),
                        aaStepYVec,
                        ndcY);

                    var (r0, r1, r2, r3) = s.IterationPerturbedSIMD(dr, di, Iterations, in _referenceOrbit);

                    Rgb24 c0 = Sample(r0),
                          c1 = Sample(r1),
                          c2 = Sample(r2),
                          c3 = Sample(r3);

                    r += c0.R + c1.R + c2.R + c3.R;
                    g += c0.G + c1.G + c2.G + c3.G;
                    b += c0.B + c1.B + c2.B + c3.B;

                    totalWeight += 4;
                }

                aaBuffer[rowOffset + x] = new Rgb24((byte)(r / totalWeight), (byte)(g / totalWeight), (byte)(b / totalWeight));
            }
        }
        #endregion
    }
}
