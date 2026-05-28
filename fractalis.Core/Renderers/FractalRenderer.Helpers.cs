using fractalis.Core.Fractals;
using fractalis.Core.Numbers;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core
{
    public partial class FractalRenderer
    {
        private double NdcY(double y)
        {
            double ndc = -((y / Height) - 0.5) * 2.0;
            if (ndc == 0.0) ndc += 0.2 / Height;
            return ndc;
        }

        private double NdcX(double x) => x * (2.0 / Height) - _aspectRatio;
        private Rgb24 Sample(IterationResult r)
        {
            if (!r.Escaped) return ColorPalette.InteriorColor.ToPixel<Rgb24>();

            double smoothIter = Fractal.GetContinousValue(r);

            System.Numerics.Vector4 c = ColorPalette.Sample(smoothIter);
            return new Rgb24((byte)(c.X * 255), (byte)(c.Y * 255), (byte)(c.Z * 255));
        }
        private Complex PixelCoordinates(double ndcX, double ndcY) => new(ndcX * _pixelSpacingDouble + _centerDouble.Real, ndcY * _pixelSpacingDouble + _centerDouble.Imaginary);

        private static int Diff(Rgb24 a, Rgb24 b) => Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) + Math.Abs(a.B - b.B);

        private bool NeedsAA(Rgb24[] colorBuffer, int x, int y)
        {
            int centerIndex = y * Width + x;
            Rgb24 center = colorBuffer[centerIndex];

            return (x > 0 && Diff(center, colorBuffer[centerIndex - 1]) > AA_THRESHOLD) ||
                   (x < Width - 1 && Diff(center, colorBuffer[centerIndex + 1]) > AA_THRESHOLD) ||
                   (y > 0 && Diff(center, colorBuffer[centerIndex - Width]) > AA_THRESHOLD) ||
                   (y < Height - 1 && Diff(center, colorBuffer[centerIndex + Width]) > AA_THRESHOLD);
        }
    }
}
