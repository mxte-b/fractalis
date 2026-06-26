using fractalis.Core.Renderers;

namespace fractalis.Core.Compositor.Layers
{
    /// <summary>
    /// Represents a context-aware composite layer with access to certain renderer parameters.
    /// </summary>
    public interface IContextAwareLayer
    {
        /// <summary>
        /// Sets the context value.
        /// </summary>
        /// <param name="ctx">The renderer context that will be used.</param>
        public void SetContext(RenderContext ctx);
    }
}
