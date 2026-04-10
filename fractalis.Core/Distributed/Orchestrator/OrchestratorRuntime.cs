using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed.Orchestrator
{
    public class OrchestratorRuntime(IOrchestratorContext ctx, ClientConnection conn) : IRuntime
    {
        private readonly IOrchestratorContext   _context = ctx;
        private readonly ClientConnection       _connection = conn;
        public async Task<MessageHandlingResult> HandleMessage(Message message)
        {
            // Logging
            if (message == null)
            {
                _context.Log("Received invalid message");
            }
            _context.Log(_connection, message is null ? "No content" : message.ToString());

            // Message handling
            switch (message)
            {
                case VideoRenderRequest renderRequest:
                    RenderJob job = new RenderJob()
                    {
                        VideoConfig = renderRequest.VideoConfig,
                        FractalRendererConfig = renderRequest.FractalRendererConfig,
                    };

                    await _context.AddJobAsync(job);
                    break;
            }

            return MessageHandlingResult.Continue;
        }
    }
}
