using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed.Orchestrator
{
    public interface IOrchestratorContext
    {
        public Task AddJobAsync(RenderJob job);
        public void Log(string message);
        public void Log(ClientConnection connection, string message);
    }
}
