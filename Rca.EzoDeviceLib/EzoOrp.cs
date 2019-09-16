using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rca.EzoDeviceLib.Objects;
using Rca.EzoDeviceLib.Specific.Ph;

namespace Rca.EzoDeviceLib
{
    /// <summary>
    /// Represents a ORP probe connected to an Atlas Scientific EZO™ ORP circuit.
    /// ORP stands for oxidation/reduction potential.
    /// https://www.atlas-scientific.com/_files/_datasheets/_circuit/ORP_EZO_datasheet.pdf
    /// </summary>
    public class EzoOrp : EzoBase
    {
        #region Constants
        private const int DEFAULT_ADDRESS = 0x62; //ORP EZO

        #endregion Constants

        #region Properties

        #endregion Properties

        #region Constructor
        /// <summary>
        /// Constructing this object will initialize it.
        /// </summary>
        /// <param name="slaveAddress">I2C slave address of EZO™ device</param>
        public EzoOrp(byte slaveAddress = DEFAULT_ADDRESS) : base(slaveAddress)
        {

        }
        
        #endregion Constructor

        #region Services
        /// <summary>
        /// Get measvalue from a single reading.
        /// </summary>
        /// <returns>ORP measvalue</returns>
        public double GetMeasValue()
        {
                WriteCommand("R");
            SpinWait.SpinUntil(() => false, 900);

            return ReadDouble();
        }

        #region Calibration
        /// <summary>
        /// Set a new calibration point.
        /// The EZO™ ORP circuit can be calibrated to any known ORP value. 
        /// </summary>
        /// <param name="value">setvalue</param>
        public void SetCalibrationPoint(int value)
        {
            WriteCommand($"cal,{value}");
            SpinWait.SpinUntil(() => false, 900);
            ReadAck(); //throws exception if failed
        }

        /// <summary>
        /// Clear all claibration data from the EZO™ device.
        /// </summary>
        public void ClearCalibration()
        {
            WriteCommand("Cal,clear");
            SpinWait.SpinUntil(() => false, PROCESSING_DELAY);
            ReadAck(); //throws exception if failed
        }

        /// <summary>
        /// Returns the number of stored calibration points
        /// </summary>
        /// <returns>Number of stored calibration points</returns>
        public int GetCalibrationInfo()
        {
            WriteCommand("Cal,?");
            SpinWait.SpinUntil(() => false, PROCESSING_DELAY);

            var response = ReadResponse();
            if (response.IsSuccessful)
            {
                try
                {
                    return int.Parse(response.Data[0]);
                }
                catch (Exception ex)
                {
                    throw new EzoResponseException(response.Code, "Invalid data", ex);
                }
            }
            else
                throw new EzoResponseException(response.Code);
        }

        /// <summary>
        /// Download calibration settings
        /// </summary>
        /// <returns>Raw calibration settings</returns>
        public byte[][] DownloadCalibration()
        {
            WriteCommand("Export,?");
            SpinWait.SpinUntil(() => false, PROCESSING_DELAY);

            var response = ReadResponse(ResponseFormat.DataWithCommand);

            if (response.Code != ResponseCode.SuccessfulRequest)
                throw new EzoResponseException(response.Code);

            try
            {
                int expCount = int.Parse(response.Data[0]);
                int expBytes = int.Parse(response.Data[1]);
                byte[][] calibData = new byte[expCount][];

                for (int i = 0; i < expCount; i++)
                {
                    var row = DownloadCalibrationRow();
                    calibData[i] = row;

                    expBytes -= row.Length;
                }

                if (expBytes > 0)
                    throw new ArgumentException("Mismatch between fetched and expected calibration data.");

                WriteCommand("Export");
                SpinWait.SpinUntil(() => false, PROCESSING_DELAY);
                var responseDone = ReadResponse(ResponseFormat.Data);

                if (responseDone.Data.Length < 1 | !string.Equals(responseDone.Data[0], "*DONE", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("No confirmation of completed export received.");

                return calibData;
            }
            catch (Exception ex)
            {
                throw new EzoResponseException(response.Code, "Fetching export info fails.", ex);
            }
        }

        /// <summary>
        /// Download one calibration row/string from calibrated device
        /// </summary>
        /// <returns>Calibration row</returns>
        private byte[] DownloadCalibrationRow()
        {
            WriteCommand("Export");
            SpinWait.SpinUntil(() => false, PROCESSING_DELAY);

            var response = ReadResponse(ResponseFormat.Unformated);

            if (response.Code == ResponseCode.SuccessfulRequest)
                return response.Payload;
            else
                throw new EzoResponseException(response.Code);
        }

        public void UploadCalibration(byte[][] data)
        {
            foreach (var row in data)
                UploadCalibration(row);
        }

        private void UploadCalibration(byte[] data)
        {
            var encoder = new ASCIIEncoding();
            var row = encoder.GetString(data);
            WriteCommand($"Import, {row}");
            SpinWait.SpinUntil(() => false, PROCESSING_DELAY);
        }

        #endregion Calibration

        #endregion Services

        #region Internal services


        #endregion Internal services
    }
}
