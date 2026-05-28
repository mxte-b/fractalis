using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Miscellaneous
{
    public record ClientSettings
    {
        public required string  DisplayName         { get; init; }
        public required Uri     OrchestratorUri     { get; init; }
        public required double  ProcessorUsageLimit { get; init; }
    }
}
