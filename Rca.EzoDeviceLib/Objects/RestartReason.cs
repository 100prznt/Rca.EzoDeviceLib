using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.EzoDeviceLib.Objects
{
    /// <summary>
    /// Reason for last restart
    /// </summary>
    public enum RestartReason
    {
        /// <summary>
        /// unknown
        /// </summary>
        [RestartReasonCode('U')]
        Unknown = 0,
        /// <summary>
        /// powered off
        /// </summary>
        [RestartReasonCode('P')]
        PoweredOff,
        /// <summary>
        /// software reset
        /// </summary>
        [RestartReasonCode('S')]
        SoftwareReset,
        /// <summary>
        /// brown out
        /// </summary>
        [RestartReasonCode('B')]
        BrownOut,
        /// <summary>
        /// watchdog
        /// </summary>
        [RestartReasonCode('W')]
        WatchDog
    }
}
