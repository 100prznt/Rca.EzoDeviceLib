﻿using Rca.EzoDeviceLib.Objects;
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

        private readonly string m_DeviceSelector;

        #endregion Members

        #region Properties
        /// <summary>
        /// I2C connection settings for the EZO™ device
        /// </summary>
        public I2cConnectionSettings Settings { get; private set; }

        /// <summary>
        /// I2C address of the current sensor connection
        /// </summary>
        public int I2CAddress { get; private set; }

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
        /// Additional information about the measured value
        /// </summary>
        public abstract MeasDataInfo ValueInfo { get; }

        #endregion Properties

        #region Constructor + Init
        /// <summary>
        /// Constructing this object will initialize it and prepare it for data retrieval.
        /// </summary>
        /// <param name="slaveAddress">I2C address of the EZO™ device</param>
        /// <param name="busSpeed">I2C bus speed (default: StandardMode</param>
        /// <param name="sharingMode">I2C sharing mode (default: Exclusive)</param>
        public EzoBase(int slaveAddress, I2cBusSpeed busSpeed = I2cBusSpeed.StandardMode, I2cSharingMode sharingMode = I2cSharingMode.Exclusive)
        {
            Settings = new I2cConnectionSettings(slaveAddress)
            {
                BusSpeed = busSpeed,
                SharingMode = sharingMode
            };
            I2CAddress = slaveAddress;

            m_DeviceSelector = I2cDevice.GetDeviceSelector();
        }

        /// <summary>
        /// Initialize sensor
        /// </summary>
        /// <returns></returns>
        public async Task InitSensorAsync()
        {
            var dis = await DeviceInformation.FindAllAsync(m_DeviceSelector);
            m_Sensor = await I2cDevice.FromIdAsync(dis[0].Id, Settings);

            await InitializeSensorAsync();
        }

        protected virtual Task InitializeSensorAsync() => Task.FromResult<object>(null);

        private static bool _isInited;

        public bool IsDevicedInited => _isInited;

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
                    var restartReason = RestartReasons.Unknown;
                    foreach (RestartReasons rr in Enum.GetValues(typeof(RestartReasons)))
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
        public virtual void Dispose()
        {
            if (m_Sensor != null)
            {
                m_Sensor.Dispose();
                m_Sensor = null;
            }
        }

        #endregion Services

        #region Internal services



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

            var response = EzoResponse.FromBuffer(buffer, ResponseFormat.Ack);

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
                    return double.Parse(response.Data[0], CultureInfo.InvariantCulture);
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
