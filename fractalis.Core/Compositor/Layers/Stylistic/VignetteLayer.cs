using System.Numerics;

namespace fractalis.Core.Compositor.Layers.Stylistic
{
    /// <summary>
    /// Represents an effect layer that applies a vignette (darkening toward edges).
    /// </summary>
    /// <param name="strength">
    /// The intensity of the vignette effect.
    /// </param>
    /// <param name="extent">
    /// The radius of the unaffected center area (higher values keep more of the image bright).
    /// </param>
    public class VignetteLayer(float strength = 10f, float extent = 0.9f) : CompositeLayer
    {
        #region JSON-exposed parameters
        public float Strength => _strength;
        public float Extent => _extent;
        #endregion

        private readonly float _strength = strength;
        private readonly float _extent = extent;

        public override void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height)
        {
            Parallel.For(0, src.Length, idx =>
            {
                (float u, float v) = Raster.IndexToUV(idx, width, height);

                float uTemp = u;
                u *= 1.0f - v;
                v *= 1.0f - uTemp;

                float vig = Math.Clamp(MathF.Pow(u * v * _strength, _extent), 0, 1);

                dst.Span[idx] = src.Span[idx] * new Vector4(vig, vig, vig, 1);
            });
        }
    }
}
