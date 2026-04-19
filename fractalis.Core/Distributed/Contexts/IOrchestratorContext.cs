using fractalis.Core.Distributed.Networking;

namespace fractalis.Core.Distributed.Contexts
{
    public interface IOrchestratorContext
    {
        public Task AddJobAsync(RenderJob job);
        public Message GetRenderAssignment();
        public void CompleteAssignment(Guid assignmentId);
        public void CancelAssignment(Guid assignmentId);
        public void Log(string message);
        public void Log(ClientConnection connection, string message);
    }
}
