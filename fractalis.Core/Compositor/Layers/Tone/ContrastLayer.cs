using System.Numerics;

namespace fractalis.Core.Compositor.Layers.Tone
{
    /// <summary>
    /// Represents an effect layer that adjusts image contrast.
    /// </summary>
    /// <param name="contrast">
    /// The contrast multiplier. 1 represents no change, values above 1 increase contrast,
    /// and values between 0 and 1 reduce contrast.
    /// </param>
    public class ContrastLayer(float contrast = 1) : CompositeLayer
    {
        #region JSON-exposed parameters
        public float Contrast => _contrast;
        #endregion

        private readonly float _contrast = contrast;

        private static readonly Vector4 _half = new(0.5f, 0.5f, 0.5f, 1f);

        public override void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height)
        {
            Parallel.For(0, src.Length, idx =>
            {
                dst.Span[idx] = (src.Span[idx] - _half) * _contrast + _half;
            });
        }
    }
}
