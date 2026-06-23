using fractalis.Core.Distributed.Contexts;
using fractalis.Core.Distributed.Networking;
using fractalis.Core.Distributed.Networking.Messages;

namespace fractalis.Core.Distributed.Runtimes
{
    public class ClientSessionRuntime(IOrchestratorContext ctx, ClientConnection conn) : IRuntime
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

                case RenderAssignmentStatusMessage assignmentStatusMessage:
                    switch (assignmentStatusMessage.Status)
                    {
                        case RenderStatus.Finished:
                            _context.CompleteAssignment(assignmentStatusMessage.AssignmentId);
                            break;

                        case RenderStatus.Failed:
                        case RenderStatus.Cancelled:
                            _context.CancelAssignment(assignmentStatusMessage.AssignmentId);
                            break;

                        default: throw new Exception("Unknown render status message.");
                    }
                    break;

                // Status message for a render job
                case RenderJobStatusMessage statusMessage:
                    break;
            }

            return MessageHandlingResult.Continue;
        }
    }
}
