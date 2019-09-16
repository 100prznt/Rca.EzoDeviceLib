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
    /// Represents a pH probe connected to an Atlas Scientific EZO™ pH Circuit.
    /// https://www.atlas-scientific.com/product_pages/circuits/ezo_ph.html
    /// </summary>
    public class EzoPh : EzoBase
    {
        #region Constants
        private const int DEFAULT_ADDRESS = 0x63; //pH EZO

        #endregion Constants

        #region Properties
        /// <summary>
        /// Get or set the temperatur for internal temperatur compensation.
        /// </summary>
        public double TemperatureCompensation
        {
            get => GetTemperatureCompensation();
            set => SetTemperatureCompensation(value);
        }

        #endregion Properties

        #region Constructor
        /// <summary>
        /// Constructing this object will initialize it.
        /// </summary>
        /// <param name="slaveAddress">I2C slave address of EZO device</param>
        public EzoPh(byte slaveAddress = DEFAULT_ADDRESS) : base(slaveAddress)
        {

        }
        
        #endregion Constructor

        #region Services
        /// <summary>
        /// Get measvalue from a single reading.
        /// </summary>
        /// <param name="temperature">Optional: Temperatur for internal temperatur compensation.</param>
        /// <returns>pH measvalue</returns>
        public double GetMeasValue(double temperature = double.NaN)
        {
            if (double.IsNaN(temperature))
                WriteCommand("R");
            else
                WriteCommand($"RT,{temperature.ToString("F2", CultureInfo.InvariantCulture)}");
            SpinWait.SpinUntil(() => false, 900);

            return ReadDouble();
        }

        /// <summary>
        /// Get slope information from characteristic curve
        /// </summary>
        /// <returns></returns>
        public Slope GetSlope()
        {
            WriteCommand("Slope,?");
            SpinWait.SpinUntil(() => false, PROCESSING_DELAY);

            var response = ReadResponse(ResponseFormat.DataWithCommand);
            if (response.IsSuccessful)
            {
                try
                {
                    return Slope.FromResponseData(response.Data);
                }
                catch (Exception ex)
                {
                    throw new EzoResponseException(response.Code, "Invalid data", ex);
                }
            }
            else
                throw new EzoResponseException(response.Code);
        }

        #region Calibration
        /// <summary>
        /// Set a new calibration point.
        /// Issuing  the  <code>SetCalibrationPoint(EzoPhCalPoint.Mid, x.xx)</code>  command  after
        /// the EZO pH circuit has been calibrated, will clear  the other  calibration points. Full
        /// calibration will have to be redone. 
        /// </summary>
        /// <param name="point">Calibration range</param>
        /// <param name="value">pH value of the buffer solution</param>
        public void SetCalibrationPoint(CalPoint point, double value)
        {
            var cmd = new StringBuilder("cal,");
            cmd.Append(point.ToString().ToLower());
            cmd.Append(",");
            cmd.Append(value.ToString("F2", CultureInfo.InvariantCulture));

            WriteCommand(cmd.ToString());
            SpinWait.SpinUntil(() => false, 900);
            ReadAck(); //throws exception if failed
        }

        /// <summary>
        /// Clear all claibration data from the EZO device.
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

        private void SetTemperatureCompensation(double temperature)
        {
            WriteCommand($"L,{temperature.ToString("F2", CultureInfo.InvariantCulture)}");
            SpinWait.SpinUntil(() => false, PROCESSING_DELAY);
        }

        private double GetTemperatureCompensation()
        {
            WriteCommand("T,?");
            SpinWait.SpinUntil(() => false, PROCESSING_DELAY);

            var response = ReadResponse(ResponseFormat.DataWithCommand);
            if (response.IsSuccessful)
            {
                try
                {
                    return double.Parse(response.Data[0]);
                }
                catch (Exception ex)
                {
                    throw new EzoResponseException(response.Code, "Invalid data", ex);
                }
            }
            else
                throw new EzoResponseException(response.Code);
        }

        #endregion Internal services
    }
}
