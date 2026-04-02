using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed
{
    /// <summary>
    /// Represents the outcome of processing a message.
    /// </summary>
    public enum MessageHandlingResult
    {
        /// <summary>
        /// Indicates that message listening should continue normally.
        /// </summary>
        Continue,

        /// <summary>
        /// Indicates that message listening should stop.
        /// </summary>
        Stop
    }
}
