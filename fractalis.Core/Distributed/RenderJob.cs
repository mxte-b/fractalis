using fractalis.Core.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed
{
    /// <summary>
    /// Represents a rendering job with associated configuration.
    /// </summary>
    public record RenderJob
    {
        /// <summary>
        /// Unique identifier of the job.
        /// </summary>
        public Guid                             Id                      { get; init; } = Guid.NewGuid();

        /// <summary>
        /// Video configuration for the job.
        /// </summary>
        public required VideoConfig             VideoConfig             { get; init; }

        /// <summary>
        /// Fractal renderer configuration to use during rendering.
        /// </summary>
        public required FractalRendererConfig   FractalRendererConfig   { get; init; }
    }
}
