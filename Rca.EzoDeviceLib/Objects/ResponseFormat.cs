using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.EzoDeviceLib.Objects
{
    public enum ResponseFormat
    {
        /// <summary>
        /// Unformated response
        /// </summary>
        Unformated = 0,
        /// <summary>
        /// Ack only
        /// (0x01 0x00)
        /// </summary>
        Ack,
        /// <summary>
        /// Comma separated data
        /// </summary>
        Data,
        /// <summary>
        /// Comma separated data with preceding command
        /// </summary>
        DataWithCommand
    }
}
