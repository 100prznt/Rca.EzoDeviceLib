using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.EzoDeviceLib.Objects
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RestartReasonCodeAttribute : Attribute
    {
        /// <summary>
        /// Restart code
        /// </summary>
        public char RestartCode { get; set; }

        /// <summary>
        /// Restart code for restart reason
        /// </summary>
        /// <param name="code">Restart code</param>
        public RestartReasonCodeAttribute(char code)
        {
            RestartCode = code;
        }
    }
}
