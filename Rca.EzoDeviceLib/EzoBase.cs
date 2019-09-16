using Rca.EzoDeviceLib.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace Rca.EzoDeviceLib
{
    /// <summary>
    /// Base class for EZO™ devices.
    /// Contains base functionality and services.
    /// </summary>
    public abstract class EzoBase : IDisposable
    {
        #region Constants
        protected const int PROCESSING_DELAY = 300;
        private const int READ_BUFFER_LENGTH = 40;

        #endregion Constants

        #region Fields
        /// <summary>
        /// Valid baud rates for UART protocol
        /// </summary>
        public static readonly int[] ValidUartBaudRates = { 300, 1200, 2400, 960, 19200, 38400, 57600, 115200 };

        #endregion Fields

        #region Members
        private I2cDevice m_Sensor { get; set; }

        private int m_I2CAddress;

        #endregion Members

        #region Properties
        /// <summary>
        /// I2C bus speed
        /// </summary>
        public I2cBusSpeed BusSpeed { get; set; }

        /// <summary>
        /// LED control
        /// </summary>
        public bool LedState
        {
            get => GetLedState();
            set => SetLedState(value);
        }

        /// <summary>
        /// Lock protocol to I2C
        /// </summary>
        public bool ProtocolLock
        {
            get => GetProtocolLock();
            set => SetProtocolLock(value);
        }

        /// <summary>
        /// I2C address of the current sensor connection
        /// </summary>
        public int I2CAddress
        {
            get => m_I2CAddress;
        }

        #endregion Properties

        #region Constructor
        /// <summary>
        /// Constructor requires a single byte 'SlaveAddress' as an argument.
        /// The typical SlaveAddress for the EZO devices you can find in datasheet.
        /// Constructing this object will initialize it and prepare it for data retrieval.
        /// </summary>
        public EzoBase(byte slaveAddress)
        {
            Init(slaveAddress);
        }

        #endregion Constructor

        #region Services

        #region Common EZO services
        /// <summary>
        /// Set the EZO device into sleep mode.
        /// Send any command to wake up the EZO device
        /// </summary>
        public void EnableSleepMode()
        {
            WriteCommand("Sleep");
            SpinWait.SpinUntil(() => false, PROCESSING_DELAY);
        }

        /// <summary>
        /// Perform a factory reset
        /// </summary>
        /// <remarks>
        /// Clears calibration
        /// LED on
        /// Response codes enabled
        /// </remarks>
        public void FactoryReset()
        {
            WriteCommand("Factory");
        }

        /// <summary>
        /// LED rapidly blinks white, used to help find device
        /// </summary>
        /// <param name="on"></param>
        public void Find(bool on = true)
        {
            if (on)
                WriteCommand("Find");
            else
                GetLedState();
        }

        /// <summary>
        /// Get device type and firmware version of the connected EZO device
        /// </summary>
        /// <returns>
        /// EZO device info
        /// </returns>
        public EzoDeviceInfo GetDeviceInfo()
        {
            WriteCommand("i");
            SpinWait.SpinUntil(() => false, PROCESSING_DELAY);

            var response = ReadResponse();
            if (response.IsSuccessful)
            {
                try
                {
                    return new EzoDeviceInfo(response.Data[0], response.Data[1]);
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
        /// Get the current device status
        /// </summary>
        /// <returns>
        /// EZO device status
        /// </returns>
        public EzoDeviceStatus GetDeviceStatus()
        {
            WriteCommand("Status");
            SpinWait.SpinUntil(() => false, PROCESSING_DELAY);

            var response = ReadResponse();
            if (response.IsSuccessful)
            {
                try
                {
                    var restartReason = RestartReason.Unknown;
                    foreach (RestartReason rr in Enum.GetValues(typeof(RestartReason)))
                    {
                        if (string.Equals(rr.GetRestartCode(), response.Data[0], StringComparison.OrdinalIgnoreCase))
                        {
                            restartReason = rr;
                            break;
                        }
                    }

                    return new EzoDeviceStatus(restartReason, double.Parse(response.Data[1], CultureInfo.InvariantCulture));
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
        /// Set I2C address on the device.
        /// EZO device perform a restart after this command
        /// </summary>
        /// <param name="address">New I2C address (1 - 127)</param>
        public void SetI2CAddress(int address)
        {
            if (address < 1 || address > 127)
                throw new ArgumentOutOfRangeException("I2C address (" + address + ") not in valid range (1 - 127).");

            WriteCommand($"I2C,{address}");
        }

        /// <summary>
        /// Set UART mode.
        /// EZO device perform a restart after this command
        /// </summary>
        /// <param name="rate">Baudrate (300, 1200, 2400, 960, 19200, 38400, 57600, 115200)</param>
        public void SetUartMode(int rate)
        {
            if (!ValidUartBaudRates.Contains(rate))
                throw new ArgumentOutOfRangeException("UART baudrate (" + rate + ") not valid.");

            WriteCommand($"Baud,{rate}");
        }

        #endregion Common EZO services

        /// <summary>
        /// Disposes the internal Sensor object when Dispose() is called on this object.
        /// </summary>
        public void Dispose()
        {
            if (m_Sensor != null)
            {
                m_Sensor.Dispose();
                m_Sensor = null;
            }
        }

        #endregion Services

        #region Internal services
        private void Init(int slaveAddress)
        {
            InitSensor(slaveAddress).Wait();
            m_I2CAddress = slaveAddress;
            //Softreset hier!
        }

        private async Task InitSensor(int slaveAddress)
        {
            var settings = new I2cConnectionSettings(slaveAddress) { BusSpeed = BusSpeed };

            string aqs = I2cDevice.GetDeviceSelector();
            var dis = await DeviceInformation.FindAllAsync(aqs);
            var di = dis[0]; //only for debugging
            m_Sensor = await I2cDevice.FromIdAsync(dis[0].Id, settings);
        }

        /// <summary>
        /// Send a ASCII formated command to the sensor
        /// </summary>
        /// <param name="asciiCmd">Command, ASCII formated</param>
        protected void WriteCommand(string asciiCmd)
        {
            if (asciiCmd.Length < 1 || asciiCmd.Length > 40)
                throw new ArgumentOutOfRangeException("Command length (" + asciiCmd.Length + ") out of valid range (1 - 40 chars)");

            byte[] cmd = Encoding.ASCII.GetBytes(asciiCmd);

            m_Sensor.Write(cmd);
        }

        protected EzoResponse ReadResponse(ResponseFormat format = ResponseFormat.DataWithCommand)
        {
            var buffer = new byte[READ_BUFFER_LENGTH];
            m_Sensor.Read(buffer);

            return EzoResponse.FromBuffer(buffer, format);
        }

        /// <summary>
        /// Read the ack byte
        /// </summary>
        /// <returns></returns>
        /// <exception cref="EzoResponseException">Get no ack</exception>
        protected bool ReadAck()
        {
            var buffer = new byte[READ_BUFFER_LENGTH];
            m_Sensor.Read(buffer);

            var response =  EzoResponse.FromBuffer(buffer, ResponseFormat.Ack);

            if (response.Code == ResponseCode.SuccessfulRequest)
                return true;
            else
                throw new EzoResponseException(response.Code);
        }


        /// <summary>
        /// Read a double value.
        /// The expected value must be on the first position of data payload.
        /// </summary>
        /// <returns>Double value</returns>
        protected double ReadDouble()
        {
            var buffer = new byte[READ_BUFFER_LENGTH];
            m_Sensor.Read(buffer);

            var response = EzoResponse.FromBuffer(buffer, ResponseFormat.Data);
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

        #region Common EZO serives, Access by properties

        private void SetLedState(bool on)
        {
            int state = on ? 1 : 0;
            WriteCommand($"L,{state}");

            SpinWait.SpinUntil(() => false, PROCESSING_DELAY);
        }

        private bool GetLedState()
        {
            WriteCommand("L,?");
            SpinWait.SpinUntil(() => false, PROCESSING_DELAY);

            var response = ReadResponse();
            if (response.IsSuccessful)
            {
                try
                {
                    return response.Data[0] == "1";
                }
                catch (Exception ex)
                {
                    throw new EzoResponseException(response.Code, "Invalid data", ex);
                }
            }
            else
                throw new EzoResponseException(response.Code);
        }
        
        private void SetProtocolLock(bool pLock)
        {
            int state = pLock ? 1 : 0;
            WriteCommand($"Plock,{state}");

            SpinWait.SpinUntil(() => false, PROCESSING_DELAY);
        }

        private bool GetProtocolLock()
        {
            WriteCommand("Plock,?");
            SpinWait.SpinUntil(() => false, PROCESSING_DELAY);

            var response = ReadResponse();
            if (response.IsSuccessful)
            {
                try
                {
                    return response.Data[0] == "1";
                }
                catch (Exception ex)
                {
                    throw new EzoResponseException(response.Code, "Invalid data", ex);
                }
            }
            else
                throw new EzoResponseException(response.Code);
        }

        #endregion Common EZO serives, Access by properties

        #endregion Internal services
    }
}
