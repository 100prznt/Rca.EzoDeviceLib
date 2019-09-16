using Rca.EzoDeviceLib.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.EzoDeviceLib
{
    /// <summary>
    /// An error occurred by reading the response data from EZO™ device.
    /// </summary>
    public class EzoResponseException : Exception
    {
        /// <summary>
        /// EZO™ device response code
        /// </summary>
        public ResponseCode ResponseCode { get; set; }


        /// <summary>
        /// Request not successful
        /// </summary>
        /// <param name="code">EZO™ device response code</param>
        public EzoResponseException(ResponseCode code) : base()
        {
            ResponseCode = code;
        }

        /// <summary>
        /// Request not successful
        /// </summary>
        /// <param name="code">EZO™ device response code</param>
        /// <param name="message">Detailed message</param>
        public EzoResponseException(ResponseCode code, string message) : base(message)
        {
            ResponseCode = code;
        }

        /// <summary>
        /// Request not successful
        /// </summary>
        /// <param name="code">EZO™ device response code</param>
        /// <param name="message">Detailed message</param>
        /// <param name="innerException">Inner exception</param>
        public EzoResponseException(ResponseCode code, string message, Exception innerException) : base(message, innerException)
        {
            ResponseCode = code;
        }
    }
}
