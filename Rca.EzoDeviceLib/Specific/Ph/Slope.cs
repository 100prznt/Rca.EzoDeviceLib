using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.EzoDeviceLib.Specific.Ph
{
    /// <summary>
    /// Slope information from characteristic curve<para/>
    /// See alos page 54 in EZO pH Ciricuit Datasheet:
    /// https://www.atlas-scientific.com/_files/_datasheets/_circuit/pH_EZO_Datasheet.pdf
    /// </summary>
    public class Slope
    {
        /// <summary>
        /// Value describes the slope of characteristic curve in the acid range (pH lower 7.0).
        /// Percentage value relative to the "ideal" characteristic curve.
        /// </summary>
        public double Acid { get; set; }

        /// <summary>
        /// Value describes the slope of characteristic curve in the base range (pH higher 7.0).
        /// Percentage value relative to the "ideal" characteristic curve.
        /// </summary>
        public double Base { get; set; }

        /// <summary>
        /// Slope information from characteristic curve
        /// </summary>
        /// <param name="acidSlope">Slope for acid range</param>
        /// <param name="baseSlope">Slope for base range</param>
        public Slope(double acidSlope, double baseSlope)
        {
            Acid = acidSlope;
            Base = baseSlope;
        }

        public static Slope FromResponseData(string[] responseData)
        {
            return new Slope(double.Parse(responseData[0], CultureInfo.InvariantCulture),
                double.Parse(responseData[1], CultureInfo.InvariantCulture));
        }
    }
}
