using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Video
{
    /// <summary>
    /// Defines easing behavior for the beginning and end of the animation.
    /// </summary>
    public record AnimationSettings()
    {
        /// <summary>
        /// Duration of the animation phase in seconds.
        /// </summary>
        public double Duration { get; init; } = 1.0;

        /// <summary>
        /// Exponent controlling easing curve. 
        /// Higher = sharper animation curve (more sudden movements).
        /// </summary>
        public double Exponent { get; init; } = 3.0;
    }
}
