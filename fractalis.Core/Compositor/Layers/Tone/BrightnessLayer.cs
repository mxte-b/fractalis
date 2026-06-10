using System.Numerics;

namespace fractalis.Core.Compositor.Layers.Tone
{
    /// <summary>
    /// Represents an effect layer that adjusts image brightness.
    /// </summary>
    /// <param name="brightness">
    /// The brightness multiplier applied to all pixels. Values above 1 increase brightness,
    /// while values between 0 and 1 decrease it.
    /// </param>
    public class BrightnessLayer(float brightness) : CompositeLayer
    {
        #region JSON-exposed parameters
        public float Brightness => brightness;
        #endregion

        private readonly Vector4 _strength = new(brightness);

        public override void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height)
        {
            Parallel.For(0, src.Length, idx =>
            {
                dst.Span[idx] = src.Span[idx] * _strength;
            });
        }
    }
}
