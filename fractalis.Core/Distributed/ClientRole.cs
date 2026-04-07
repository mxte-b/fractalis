using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed
{
    /// <summary>
    /// Defines the role of a client in the system.
    /// </summary>
    public enum ClientRole
    {
        /// <summary>
        /// Client that initiates work.
        /// </summary>
        Initiator,

        /// <summary>
        /// Client that executes assigned tasks.
        /// </summary>
        Worker
    }
}
