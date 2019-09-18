using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rca.EzoDeviceLib.Specific.Rtd
{
    public static class TemperatureScaleExtensions
    {
        public static string GetScaleCode(this TemperatureScales scale)
        {
            Attribute[] attributes = scale.GetAttributes();

            TemperatureScaleAttribute attr = null;

            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i].GetType() == typeof(TemperatureScaleAttribute))
                {
                    attr = (TemperatureScaleAttribute)attributes[i];
                    break;
                }
            }

            if (attr == null)
                return scale.ToString();
            else
                return attr.ScaleCode.ToString();
        }

        public static string GetName(this TemperatureScales scale)
        {
            Attribute[] attributes = scale.GetAttributes();

            TemperatureScaleAttribute attr = null;

            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i].GetType() == typeof(TemperatureScaleAttribute))
                {
                    attr = (TemperatureScaleAttribute)attributes[i];
                    break;
                }
            }

            if (attr == null)
                return scale.ToString();
            else
                return attr.Name;
        }

        public static string GetSymbol(this TemperatureScales scale)
        {
            Attribute[] attributes = scale.GetAttributes();

            TemperatureScaleAttribute attr = null;

            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i].GetType() == typeof(TemperatureScaleAttribute))
                {
                    attr = (TemperatureScaleAttribute)attributes[i];
                    break;
                }
            }

            if (attr == null)
                return scale.ToString();
            else
                return attr.Symbol;
        }

        private static Attribute[] GetAttributes(this TemperatureScales restartReason)
        {
            var fi = restartReason.GetType().GetField(restartReason.ToString());
            Attribute[] attributes = (Attribute[])fi.GetCustomAttributes(typeof(Attribute), false);

            return attributes;
        }
    }
}
