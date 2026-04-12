using fractalis.Core.Distributed.Networking;

namespace fractalis.Core.Distributed.Contexts
{
    public interface IClientContext
    {
        Task SendMessageToServerAsync(Message message);
    }
}
