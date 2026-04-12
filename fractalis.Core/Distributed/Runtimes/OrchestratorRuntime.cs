using fractalis.Core.Distributed.Contexts;
using fractalis.Core.Distributed.Networking;

namespace fractalis.Core.Distributed.Runtimes
{
    public class OrchestratorRuntime(IOrchestratorContext ctx, ClientConnection conn) : IRuntime
    {
        private readonly IOrchestratorContext _context = ctx;
        private readonly ClientConnection _connection = conn;
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
                // New video render request
                case VideoRenderRequest renderRequest:
                    await _context.AddJobAsync(new RenderJob()
                    {
                        UploadUri = renderRequest.UploadUri,
                        VideoConfig = renderRequest.VideoConfig,
                        FractalRendererConfig = renderRequest.FractalRendererConfig,
                    });
                    break;

                // Worker requesting work
                case RenderAssignmentRequest:
                    await _connection.SendMessageAsync(_context.GetRenderAssignment());
                    break;

                // Status message for a render job
                case RenderJobStatusMessage statusMessage:
                    break;
            }

            return MessageHandlingResult.Continue;
        }
    }
}
