using fractalis.Core.Distributed.Contexts;
using fractalis.Core.Distributed.Networking;
using fractalis.Core.Distributed.Networking.Messages;

namespace fractalis.Core.Distributed.Runtimes
{
    public class InitiatorRuntime(IClientContext context) : IRuntime
    {
        private IClientContext _context = context;

        public Task<MessageHandlingResult> HandleMessage(Message message)
        {
            if (message is RenderJobStatusMessage statusMessage)
            {
                Console.WriteLine($"New status: {statusMessage.Status}");

                return Task.FromResult(MessageHandlingResult.Stop);
            }

            return Task.FromResult(MessageHandlingResult.Continue);
        }
    }
}
