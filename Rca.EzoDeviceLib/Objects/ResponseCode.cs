using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.EzoDeviceLib.Objects
{
    /// <summary>
    /// EZO device response codes
    /// </summary>
    public enum ResponseCode : byte
    {
        /// <summary>
        /// Unkown, not set
        /// </summary>
        Unknown = 0x00,
        /// <summary>
        /// Successful request
        /// </summary>
        SuccessfulRequest = 0x01,
        /// <summary>
        /// Syntax error
        /// </summary>
        SyntaxError = 0x02,
        /// <summary>
        /// Still processing, not ready
        /// </summary>
        StillProcessing = 0xFE,
        /// <summary>
        /// No data to send
        /// </summary>
        NoDataToSend = 0xFF,
    }
}
