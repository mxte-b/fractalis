using fractalis.Core.Compositor.Layers;
using fractalis.Core.Compositor.Layers.Color;
using fractalis.Core.Converters;
using fractalis.Core.Renderers;
using SixLabors.ImageSharp.PixelFormats;
using System.Buffers;
using System.Numerics;
using System.Text.Json.Serialization;

namespace fractalis.Core.Compositor
{
    [JsonConverter(typeof(LayerCompositorConverter))]
    public class LayerCompositor(List<CompositeLayer>? layers = null)
    {
        internal List<CompositeLayer> _layers = layers ?? [];

        /// <summary>
        /// Appends an <see cref="ICompositeLayer"/> to the compositor.
        /// </summary>
        /// <param name="layer">The layer to append.</param>
        /// <returns>A reference to this instance for chaining.</returns>
        public LayerCompositor AddLayer(CompositeLayer layer)
        {
            _layers.Add(layer);

            return this;
        }

        public void Apply(Rgba32[] buffer, int width, int height, RenderContext ctx)
        {
            if (_layers.Count == 0) return;

            // Converting to linear color space
            var rent = ArrayPool<Vector4>.Shared.Rent(buffer.Length);
            var rent2 = ArrayPool<Vector4>.Shared.Rent(buffer.Length);

            // Since Rent() can give a larger array than what we ask for,
            // we slice it to the required length.
            var src = rent.AsMemory(0, buffer.Length);
            var dst = rent2.AsMemory(0, buffer.Length);

            try
            {
                // Converting to linear space
                ColorUtility.ToLinearSpace(buffer, src);

                // Applying all layers in sequence
                foreach (var layer in _layers)
                {
                    // Update render context for context-aware layers
                    if (layer is IContextAwareLayer ctxLayer) ctxLayer.SetContext(ctx);
                    
                    layer.Apply(src, dst, width, height);
                    (src, dst) = (dst, src);
                }

                // Converting back to sRGB
                ColorUtility.TosRGBSpace(src, buffer);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while applying layers: {e}");
            }
            // Always return the rented array
            finally
            {
                ArrayPool<Vector4>.Shared.Return(rent);
                ArrayPool<Vector4>.Shared.Return(rent2);
            }
        }
    }
}
