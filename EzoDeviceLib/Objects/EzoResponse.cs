using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.EzoDeviceLib.Objects
{
    /// <summary>
    /// Response data container for EZO devices
    /// </summary>
    public class EzoResponse
    {
        /// <summary>
        /// Responsecode
        /// </summary>
        public ResponseCode Code { get; private set; }

        /// <summary>
        /// Request was successful
        /// </summary>
        public bool IsSuccessful { get => Code == ResponseCode.SuccessfulRequest; }

        /// <summary>
        /// Command string
        /// </summary>
        public string Command { get; private set; } = null;

        /// <summary>
        /// Response data array
        /// </summary>
        public string[] Data { get; private set; }

        /// <summary>
        /// Plain response data, without head and control information
        /// </summary>
        public byte[] Payload { get; set; }

        /// <summary>
        /// Generate a <see cref="EzoResponse"/> object from a bytearray (reading buffer).
        /// </summary>
        /// <param name="buffer">Reading buffer</param>
        /// <param name="format">Expected data format</param>
        /// <returns>Response data as <see cref="EzoResponse"/> object</returns>
        public static EzoResponse FromBuffer(byte[] buffer, ResponseFormat format)
        {
            var response = new EzoResponse();

            try
            {
                response.Code = (ResponseCode)buffer[0];
            }
            catch (Exception)
            {
                throw new ArgumentException("Undefined response code (" + buffer[0] + ")");
            }

            byte[] payload = new byte[buffer.Length - 1];

            Array.Copy(buffer, 1, payload, 0, payload.Length); //ignore ack at buffer[0]
            int lastIndex = Array.FindLastIndex(payload, b => b != 0x00);
            Array.Resize(ref payload, lastIndex + 1);
            
            //var responseString = Encoding.ASCII.GetString(buffer, 1, buffer.Length - 1); //ignore ack at buffer[0]
            var responseString = Encoding.ASCII.GetString(payload);
            var responseSegments = responseString.Trim('\0').Split(',');
            response.Payload = payload;


            if (format == ResponseFormat.Ack || responseSegments.Length == 0)
                return response;

            if (format == ResponseFormat.Unformated)
            {
                response.Data = new string[1] { responseString.Trim('\0') };
                return response;
            }

            int i = 0;
            if (format == ResponseFormat.DataWithCommand && responseSegments.Length >= 1)
            {
                response.Command = responseSegments[0];
                i = 1;
            }

            response.Data = new string[responseSegments.Length - i];
            Array.Copy(responseSegments, i, response.Data, 0, responseSegments.Length - i);

            return response;
        }
    }
}
