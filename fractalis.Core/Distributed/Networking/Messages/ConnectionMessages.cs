using fractalis.Core.Distributed.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed.Networking.Messages
{
    /// <summary>
    /// Message sent by a client to register itself with the orchestrator.
    /// </summary>
    public record RegistrationMessage : Message
    {
        /// <summary>
        /// The display name of the client for identification purposes.
        /// </summary>
        public required string  DisplayName { get; init; }

        public ClientRole       Role        { get; init; } = ClientRole.Worker;
    }

    /// <summary>
    /// Message sent by the orchestrator to acknowledge a client's registration.
    /// </summary>
    public record RegistrationAcknowledgedMessage : Message
    {
        /// <summary>
        /// The unique identifier of the client.
        /// </summary>
        public required Guid    ClientId    { get; init; }
    }

    /// <summary>
    /// Message sent by a client to reconnect using its existing client ID.
    /// </summary>
    public record ReconnectMessage : RegistrationAcknowledgedMessage;
}
