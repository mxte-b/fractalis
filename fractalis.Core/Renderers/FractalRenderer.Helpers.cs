using fractalis.Core.Fractals;
using fractalis.Core.Numbers;
using SixLabors.ImageSharp.PixelFormats;

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
        private Rgba32 Sample(IterationResult r)
        {
            if (!r.Escaped) return ColorPalette.InteriorColor.ToPixel<Rgba32>();

            double smoothIter = Fractal.GetContinousValue(r);

            System.Numerics.Vector4 c = ColorPalette.Sample(smoothIter);
            return new Rgba32((byte)(c.X * 255), (byte)(c.Y * 255), (byte)(c.Z * 255));
        }
        private Complex PixelCoordinates(double ndcX, double ndcY) => new(ndcX * _pixelSpacingDouble + _centerDouble.Real, ndcY * _pixelSpacingDouble + _centerDouble.Imaginary);

        private static int Diff(Rgba32 a, Rgba32 b) => Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) + Math.Abs(a.B - b.B);

        private bool NeedsAA(Rgba32[] colorBuffer, int x, int y)
        {
            int centerIndex = y * Width + x;
            Rgba32 center = colorBuffer[centerIndex];

            return (x > 0 && Diff(center, colorBuffer[centerIndex - 1]) > AA_THRESHOLD) ||
                   (x < Width - 1 && Diff(center, colorBuffer[centerIndex + 1]) > AA_THRESHOLD) ||
                   (y > 0 && Diff(center, colorBuffer[centerIndex - Width]) > AA_THRESHOLD) ||
                   (y < Height - 1 && Diff(center, colorBuffer[centerIndex + Width]) > AA_THRESHOLD);
        }
    }
}
