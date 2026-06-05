using System.Numerics;

namespace fractalis.Core.Compositor.Layers.Color
{
    /// <summary>
    /// Represents an effect layer for hue-shifting.
    /// </summary>
    /// <param name="shiftDegrees">The hue shift in range [0,360]</param>
    public class HueShiftLayer(float shiftDegrees) : CompositeLayer
    {
        #region JSON-exposed parameters
        public float ShiftDegrees => shiftDegrees;
        #endregion

        private readonly float _shift = (shiftDegrees / 360f) % 1f;

        public override void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height)
        {
            Parallel.For(0, src.Length, idx =>
            {
                var pixel = src.Span[idx];

                ColorUtility.RGBToHSV_Inplace(ref pixel);

                pixel.X = (pixel.X + _shift) % 1f;
                if (pixel.X < 0) pixel.X += 1f;

                ColorUtility.HSVToRGB_Inplace(ref pixel);

                dst.Span[idx] = pixel;
            });
        }
    }
}
