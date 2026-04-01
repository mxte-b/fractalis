using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed
{
    internal class ClientRuntime
    {
        public async Task HandleMessage(Message message)
        {
            Console.WriteLine(JsonSerializer.Serialize(message));
        }
    }
}
