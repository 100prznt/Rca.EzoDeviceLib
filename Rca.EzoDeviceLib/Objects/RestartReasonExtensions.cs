using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rca.EzoDeviceLib.Objects
{
    public static class RestartReasonExtensions
    {
        public static string GetRestartCode(this RestartReason restartReason)
        {
            Attribute[] attributes = restartReason.GetAttributes();

            RestartReasonCodeAttribute attr = null;

            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i].GetType() == typeof(RestartReasonCodeAttribute))
                {
                    attr = (RestartReasonCodeAttribute)attributes[i];
                    break;
                }
            }

            if (attr == null)
                return restartReason.ToString();
            else
                return attr.RestartCode.ToString();
        }

        private static Attribute[] GetAttributes(this RestartReason restartReason)
        {
            var fi = restartReason.GetType().GetField(restartReason.ToString());
            Attribute[] attributes = (Attribute[])fi.GetCustomAttributes(typeof(Attribute), false);

            return attributes;
        }
    }
}
