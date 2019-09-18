using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.EzoDeviceLib.Specific.Rtd
{
    /// <summary>
    /// Extended information about a temperature scale.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class TemperatureScaleAttribute : Attribute
    {
        /// <summary>
        /// Hardware code to set and read the scale.
        /// </summary>
        public char ScaleCode { get; set; }

        /// <summary>
        /// Human-readable name of the scale.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Symbol of the scale.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Extended information about a temperature scale.
        /// </summary>
        /// <param name="code">Hardware code to set and read the scale.</param>
        /// <param name="name">Human-readable name of the scale.</param>
        /// <param name="symbol">Symbol of the scale.</param>
        public TemperatureScaleAttribute(char code, string name, string symbol)
        {
            ScaleCode = code;
            Name = name;
            Symbol = symbol;
        }
    }
}
