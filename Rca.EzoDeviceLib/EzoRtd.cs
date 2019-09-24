using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rca.EzoDeviceLib.Events;
using Rca.EzoDeviceLib.Objects;
using Rca.EzoDeviceLib.Specific.Ph;
using Rca.EzoDeviceLib.Specific.Rtd;

namespace Rca.EzoDeviceLib
{
    /// <summary>
    /// Represents a PT100/PT1000 probe connected to an Atlas Scientific EZO™ RTD Circuit.
    /// https://www.atlas-scientific.com/product_pages/circuits/ezo_rtd.html
    /// </summary>
    public class EzoRtd : EzoTimedBase
    {
        #region Constants
        /// <summary>
        /// Factory I2C adress of the EZO™ RTD Circuit
        /// </summary>
        /// <remarks>Public for external usage, e.g. to prefill setting files.</remarks>
        public const int DEFAULT_ADDRESS = 0x66;

        #endregion Constants

        #region Properties
        /// <summary>
        /// Get or set the temperatur scale.
        /// </summary>
        public TemperatureScales TemperatureCompensation
        {
            get => GetTemperatureScale();
            set => SetTemperatureScale(value);
        }

        /// <summary>
        /// Additional information about the measured value
        /// </summary>
        public override MeasDataInfo ValueInfo
        {
            get
            {
                var scale = GetTemperatureScale();
                return new MeasDataInfo("Temperature", scale.GetName(), scale.GetSymbol());
            }
        }

        /// <summary>
        /// RaiseEventsOnUIThread
        /// </summary>
        public bool RaiseEventsOnUIThread { get; set; } = true;

        #endregion Properties

        #region Constructor - Init
        /// <summary>
        /// 
        /// </summary>
        /// <param name="slaveAddress"></param>
        public EzoRtd(byte slaveAddress)
            : base(slaveAddress, new TimeSpan(), ReadingMode.Manual)
        { }

        /// <summary>
        /// Initializes a new EzoRtd object using the continuous reading mode.
        /// </summary>
        /// <param name="slaveAddress">I2C address of the EZO™ RTD Circuit</param>
        /// <param name="readInterval">interval for continuous reading</param>
        /// <param name="modeR">Reading mode (default: Continuous)</param>
        public EzoRtd(byte slaveAddress, TimeSpan readInterval, ReadingMode modeR = ReadingMode.Continuous)
            : base(slaveAddress, readInterval, modeR)
        { }

        #endregion Constructor

        #region Services
        /// <summary>
        /// Get measvalue from a single reading.
        /// </summary>
        /// <returns>temperature measvalue in Celcius</returns>
        public async Task<double> GetMeasValue()
        {
            WriteCommand("R");

            // Wait for senzor to measure
            Task.Delay(600).Wait();

            return ReadDouble();
        }

        #region Calibration
        /// <summary>
        /// Set thecalibration point.
        /// EZO RTD circuit uses single point calibration.
        /// </summary>
        /// <param name="value">reference temperature</param>
        public void SetCalibrationPoint(double value)
        {
            var cmd = new StringBuilder("Cal,");
            cmd.Append(value.ToString("F2", CultureInfo.InvariantCulture));

            WriteCommand(cmd.ToString());
            SpinWait.SpinUntil(() => false, 900);
            ReadAck(); //throws exception if failed
        }

        /// <summary>
        /// Clear claibration data from the EZO device.
        /// </summary>
        public void ClearCalibration()
        {
            WriteCommand("Cal,clear");
            SpinWait.SpinUntil(() => false, PROCESSING_DELAY);
            ReadAck(); //throws exception if failed
        }

        /// <summary>
        /// Returns the number of stored calibration points (1 or 0)
        /// </summary>
        /// <returns>Number of stored calibration points (1 or 0)</returns>
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
        /// <summary>
        /// Override and initialize timer in EzoTimedBase
        /// </summary>
        /// <returns></returns>
        protected override Task InitializeSensorAsync()
        {
            base.InitializeTimer();

            return Task.FromResult<object>(null);
        }

        private void SetTemperatureScale(TemperatureScales scale)
        {
            WriteCommand($"S,{scale.GetScaleCode()}");
            SpinWait.SpinUntil(() => false, PROCESSING_DELAY);
        }

        private TemperatureScales GetTemperatureScale()
        {
            WriteCommand("S,?");
            SpinWait.SpinUntil(() => false, PROCESSING_DELAY);

            var response = ReadResponse(ResponseFormat.DataWithCommand);
            if (response.IsSuccessful)
            {
                try
                {
                    foreach (TemperatureScales ts in Enum.GetValues(typeof(TemperatureScales)))
                    {
                        if (string.Equals(ts.GetScaleCode(), response.Data[0], StringComparison.OrdinalIgnoreCase))
                            return ts;
                    }
                    throw new EzoResponseException(response.Code, "Invalid data for \"S,?\" command: " + response.Data[0]);
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
        /// On Timer Tick 
        /// </summary>
        protected override void OnTimerAsync()
        {
            // Read temperature value.
            var temperature = GetMeasValue().Result;

            // Fire ValueChanged Event
            RaiseEventHelper.CheckRaiseEventOnUIThread(this, ValueChanged, new EzoRTD_EventArgs(temperature), RaiseEventsOnUIThread);
        }

        #endregion Internal services

        #region Events
        /// <summary>
        /// Value has changed event
        /// </summary>
        public event EventHandler<EzoRTD_EventArgs> ValueChanged;

        #endregion Events
    }
}
