using System.Numerics;

namespace fractalis.Core.Compositor.Layers.Color
{
    /// <summary>
    /// Represents an effect layer for color temperature adjustment.
    /// </summary>
    /// <param name="kelvin">
    /// The target color temperature in Kelvin (from 1000K to 10000K). Lower values produce warmer tones,
    /// while higher values produce cooler tones.
    /// </param>
    public class TemperatureLayer(float kelvin) : CompositeLayer
    {
        #region JSON-exposed parameters
        public float Kelvin => _kelvin;
        #endregion

        private readonly float _kelvin = kelvin;

        private static readonly Vector4[] _kelvinLUT =
        [
            new(1.0000000f, 0.0395462f, 0.0000000f, 1f), // 1000K
            new(1.0000000f, 0.1529262f, 0.0000000f, 1f), // 1500K
            new(1.0000000f, 0.2501584f, 0.0060488f, 1f), // 2000K
            new(1.0000000f, 0.3564003f, 0.0648033f, 1f), // 2500K
            new(1.0000000f, 0.4564111f, 0.1470273f, 1f), // 3000K
            new(1.0000000f, 0.5520115f, 0.2501584f, 1f), // 3500K
            new(1.0000000f, 0.6375970f, 0.3662527f, 1f), // 4000K
            new(1.0000000f, 0.7083759f, 0.4910209f, 1f), // 4500K
            new(1.0000000f, 0.7758223f, 0.6172066f, 1f), // 5000K
            new(1.0000000f, 0.8387991f, 0.7454044f, 1f), // 5500K
            new(1.0000000f, 0.8962694f, 0.8631573f, 1f), // 6000K
            new(1.0000000f, 0.9473066f, 0.9822506f, 1f), // 6500K
            new(0.9130987f, 0.8962694f, 1.0000000f, 1f), // 7000K
            new(0.8307700f, 0.8549927f, 1.0000000f, 1f), // 7500K
            new(0.7681513f, 0.8148467f, 1.0000000f, 1f), // 8000K
            new(0.7156937f, 0.7835379f, 1.0000000f, 1f), // 8500K
            new(0.6724432f, 0.7529423f, 1.0000000f, 1f), // 9000K
            new(0.6307572f, 0.7304609f, 1.0000000f, 1f), // 9500K
            new(0.6038274f, 0.7083759f, 1.0000000f, 1f), // 10000K
        ];

        private static Vector4 SampleLUT(float kelvin)
        {
            float t = Math.Clamp((kelvin - 1000f) / 500f, 0f, _kelvinLUT.Length - 1);

            int left = (int)t;
            int right = Math.Min(left + 1, _kelvinLUT.Length - 1);

            return Vector4.Lerp(_kelvinLUT[left], _kelvinLUT[right], t - left);
        }

        public override void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height)
        {
            Vector4 kelvin = SampleLUT(_kelvin);

            Parallel.For(0, src.Length, idx =>
            {
                dst.Span[idx] = src.Span[idx] * kelvin;
            });
        }
    }
}
