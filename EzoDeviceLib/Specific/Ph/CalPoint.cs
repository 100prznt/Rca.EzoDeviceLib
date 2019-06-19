using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.EzoDeviceLib.Specific.Ph
{
    public enum CalPoint
    {
        /// <summary>
        /// Midrage calibration point.
        /// Default value is 7.00 pH
        /// </summary>
        Mid = 0,
        /// <summary>
        /// Lower calibration point.
        /// Default value is 4.00 pH
        /// </summary>
        Low,
        /// <summary>
        /// Upper calibration point.
        /// Default value is 10.00 pH
        /// </summary>
        High
    }
}
