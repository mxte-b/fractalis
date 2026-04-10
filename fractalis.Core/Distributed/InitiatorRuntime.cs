using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed
{
    internal class InitiatorRuntime : IRuntime
    {
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
