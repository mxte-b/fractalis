using System.Numerics;

namespace fractalis.Core.Compositor.Layers.Color
{
    /// <summary>
    /// Represents an effect layer that adjusts color saturation.
    /// </summary>
    /// <param name="saturation">
    /// The saturation multiplier. Values above 1 increase saturation,
    /// while values between 0 and 1 decrease it.
    /// </param>
    public class SaturationLayer(float saturation) : CompositeLayer
    {
        #region JSON-exposed parameters
        public float Saturation => _saturation;
        #endregion

        private readonly float _saturation = saturation;

        public override void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height)
        {
            Parallel.For(0, src.Length, idx =>
            {
                var pixel = src.Span[idx];

                ColorUtility.RGBToHSV_Inplace(ref pixel);

                pixel.Y = Math.Clamp(pixel.Y * _saturation, 0, 1);

                ColorUtility.HSVToRGB_Inplace(ref pixel);

                dst.Span[idx] = pixel;
            });
        }
    }
}
