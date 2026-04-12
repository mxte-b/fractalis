using fractalis.Core.Distributed.Networking;

namespace fractalis.Core.Distributed.Contexts
{
    public interface IOrchestratorContext
    {
        public Task AddJobAsync(RenderJob job);
        public Message GetRenderAssignment();
        public void Log(string message);
        public void Log(ClientConnection connection, string message);
    }
}
