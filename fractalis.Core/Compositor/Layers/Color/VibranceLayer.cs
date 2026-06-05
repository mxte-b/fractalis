using System.Numerics;

namespace fractalis.Core.Compositor.Layers.Color
{
    /// <summary>
    /// Represent a composite layer for vibrancy effect.
    /// </summary>
    /// <param name="vibrance">The strength of the effect in the range [0,1].</param>
    public class VibranceLayer(float vibrance) : CompositeLayer
    {
        #region JSON-exposed parameters
        public float Vibrance => vibrance;
        #endregion

        private readonly float _vibrance = Math.Clamp(vibrance, 0, 1);

        public override void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height)
        {
            Parallel.For(0, src.Length, idx =>
            {
                var pixel = src.Span[idx];

                ColorUtility.RGBToHSV_Inplace(ref pixel);

                float vibranceBoost = _vibrance * (1f - pixel.Y);
                pixel.Y = Math.Clamp(pixel.Y + vibranceBoost, 0, 1);

                ColorUtility.HSVToRGB_Inplace(ref pixel);

                dst.Span[idx] = pixel;
            });
        }
    }
}
