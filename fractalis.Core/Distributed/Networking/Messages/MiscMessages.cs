using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed.Networking.Messages
{
    /// <summary>
    /// Message containing rendered image data for a specific frame.
    /// </summary>
    public record RenderedImageMessage : Message
    {
        /// <summary>
        /// Index of the rendered frame.
        /// </summary>
        public required int     FrameIndex  { get; init; }

        /// <summary>
        /// Raw image data in byte form.
        /// </summary>
        public required byte[]  Bytes       { get; init; }
    }

    /// <summary>
    /// Message used for debugging purposes, containing arbitrary text content.
    /// </summary>
    public record DebugMessage : Message
    {
        /// <summary>
        /// The debug message content.
        /// </summary>
        public required string  Content     { get; init; }
    }
}
