using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.EzoDeviceLib.Objects
{
    /// <summary>
    /// Container with EZO™ device information
    /// </summary>
    public class EzoDeviceInfo
    {
        /// <summary>
        /// EZO device type
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// EZO device firmware version
        /// </summary>
        public string FirmwareVersion { get; set; }

        /// <summary>
        /// Construct a new <see cref="EzoDeviceInfo"/> with provided informations
        /// </summary>
        /// <param name="deviceType">EZO device type</param>
        /// <param name="fwVersion">EZO device firmware version</param>
        public EzoDeviceInfo(string deviceType, string fwVersion)
        {
            DeviceType = deviceType;
            FirmwareVersion = fwVersion;
        }
    }
}
