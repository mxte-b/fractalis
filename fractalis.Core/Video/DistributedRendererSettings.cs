using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Video
{
    public record DistributedRendererSettings
    {
        public required Uri                     OrchestratorUri         { get; init; }
        public int                              FrameListenerPort       { get; init; } = 8060;
    }
}
