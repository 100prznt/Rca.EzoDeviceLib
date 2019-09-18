using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.EzoDeviceLib.Objects
{
    /// <summary>
    /// Container with EZO device status
    /// </summary>
    public class EzoDeviceStatus
    {
        /// <summary>
        /// Resatart code (reason for restart)
        /// </summary>
        public RestartReasons RestartCode { get; set; }

        /// <summary>
        /// Voltage at Vcc [V]
        /// </summary>
        public double VccVoltage { get; set; }

        /// <summary>
        /// Construct a new <see cref="EzoDeviceStatus"/> with provided informations
        /// </summary>
        /// <param name="restartCode">Resatart code (reason for restart)</param>
        /// <param name="vccVoltage">Voltage at Vcc [V]</param>
        public EzoDeviceStatus(RestartReasons restartCode, double vccVoltage)
        {
            RestartCode = restartCode;
            VccVoltage = vccVoltage;
        }
    }
}
