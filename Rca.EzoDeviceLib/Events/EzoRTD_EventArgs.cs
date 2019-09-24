using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.EzoDeviceLib.Events
{
    /// <summary>
    /// This is EventArgs for EzoRTD board
    /// </summary>
    public class EzoRTD_EventArgs : EventArgs
    {
        /// <summary>
        /// Event Value
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// EventArgs
        /// </summary>
        /// <param name="temp"></param>
        public EzoRTD_EventArgs(double temp)
        {
            Temperature = temp;
        }
    }
}
