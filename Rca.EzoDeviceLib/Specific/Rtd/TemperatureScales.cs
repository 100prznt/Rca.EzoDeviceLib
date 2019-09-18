using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.EzoDeviceLib.Specific.Rtd
{
    /// <summary>
    /// Available temperature scales
    /// </summary>
    /// <remarks></remarks>
    public enum TemperatureScales
    {
        /// <summary>
        /// Celsius
        /// </summary>
        [TemperatureScale('c', "Celsius", "°C")]
        Celsius,
        /// <summary>
        /// Kelvin
        /// </summary>
        [TemperatureScale('k', "Kelvin", "K")]
        Kelvin,
        /// <summary>
        /// Fahrenheit
        /// </summary>
        [TemperatureScale('f', "Fahrenheit", "°F")]
        Fahrenheit
    }
}
