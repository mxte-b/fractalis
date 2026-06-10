using System.Numerics;

namespace fractalis.Core.Compositor.Layers.Stylistic
{
    /// <summary>
    /// Represents an effect layer that applies a vignette (darkening toward edges).
    /// </summary>
    /// <param name="strength">
    /// Controls how bright the center stays. Higher values preserve more brightness;
    /// lower values darken the image more aggressively. Practical range: 5–25.
    /// </param>
    /// <param name="extent">
    /// Controls the softness of the falloff. Below 1 gives a gradual fade;
    /// above 1 gives a sharper edge. Practical range: 0.3–1.5.
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
