using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.EzoDeviceLib
{
    /// <summary>
    /// Provides additional information about the measured value.
    /// </summary>
    public class MeasDataInfo
    {
        /// <summary>
        /// Name of the measured value
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Unit of the measured value
        /// </summary>
        public string Unit { get; private set; }

        /// <summary>
        /// Short symbol of the measured value (e.g. °C, V)
        /// </summary>
        public string Symbol { get; private set; }

        /// <summary>
        /// Default costructor
        /// </summary>
        /// <param name="name">Name of the measured value</param>
        /// <param name="unit">Unit of the measured value</param>
        /// <param name="symbol"></param>
        public MeasDataInfo(string name, string unit, string symbol)
        {
            Name = name;
            Unit = unit;
            Symbol = symbol;
        }
    }
}
