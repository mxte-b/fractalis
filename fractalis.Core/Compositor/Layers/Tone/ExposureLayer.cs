using System.Numerics;

namespace fractalis.Core.Compositor.Layers.Tone
{
    /// <summary>
    /// Represents an effect layer that applies exposure adjustment.
    /// </summary>
    /// <param name="exposure">
    /// The exposure value applied to the image. Positive values increase brightness,
    /// while negative values darken the image.
    /// </param>
    public class ExposureLayer(float exposure) : CompositeLayer
    {
        #region JSON-exposed parameters
        public float Exposure => _exposure;
        #endregion

        private readonly float _exposure = exposure;

        public override void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height)
        {
            Parallel.For(0, src.Length, idx =>
            {
                dst.Span[idx] = src.Span[idx] * _exposure;
            });
        }
    }
}
