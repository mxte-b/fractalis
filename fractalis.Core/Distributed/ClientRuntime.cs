using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed
{
    /// <summary>
    /// Handles incoming messages and rendering for a client instance.
    /// </summary>
    internal class ClientRuntime
    {
        /// <summary>
        /// Processes an incoming <see cref="Message"/> by serializing it to the console.
        /// </summary>
        /// <param name="message">The message to handle. Must not be <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task HandleMessage(Message message)
        {
            // Currently just prints the JSON representation to the console
            Console.WriteLine(message);
        }
    }
}