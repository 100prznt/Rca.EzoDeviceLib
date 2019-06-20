using Rca.EzoDeviceLib.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.EzoDeviceLib
{
    /// <summary>
    /// An error occurred by reading the response data from EZO device.
    /// </summary>
    public class EzoResponseException : Exception
    {
        public ResponseCode ResponseCode { get; set; }

        public EzoResponseException(ResponseCode code) : base()
        {
            ResponseCode = code;
        }
        public EzoResponseException(ResponseCode code, string message) : base(message)
        {
            ResponseCode = code;
        }
        public EzoResponseException(ResponseCode code, string message, Exception innerException) : base(message, innerException)
        {
            ResponseCode = code;
        }
    }
}
