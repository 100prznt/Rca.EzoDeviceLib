using Rca.EzoDeviceLib.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace Rca.EzoDeviceLib
{
    /// <summary>
    /// Extends the <seealso cref="EzoBase"/> class to functionality for continuous timed reading.
    /// </summary>
    public abstract class EzoTimedBase : EzoBase
    {
        #region Members
        private readonly Timer m_Timer;

        private ReadingMode m_ReadingMode;

        private TimeSpan m_ReadInterval;

        #endregion Members

        #region Properties
        /// <summary>
        /// Current reading mode
        /// </summary>
        public ReadingMode ReadingMode
        {
            get => m_ReadingMode; 
            set => this.Initialize(value);
        }

        /// <summary>
        /// Interval for continuous timed reading
        /// </summary>
        public TimeSpan ReadInterval
        {
            get => m_ReadInterval; 
            set
            {
                if (ReadingMode == ReadingMode.Manual)
                    throw new NotSupportedException($"You cannot change {nameof(ReadInterval)} when {nameof(ReadingMode)} is set to {ReadingMode.Manual}.");

                m_ReadInterval = value;
                m_Timer?.Change(0, (int)m_ReadInterval.TotalMilliseconds);
            }
        }

        #endregion Properties

        #region Constructor
        /// <summary>
        /// Constructor to use the continuous reading mode.
        /// </summary>
        /// <param name="slaveAddress">I2C address of the EZO™ RTD Circuit</param>
        /// <param name="readInterval">interval for continuous reading</param>
        /// <param name="modeR">Reading mode (default: Continuous)</param>
        public EzoTimedBase(byte slaveAddress, TimeSpan readInterval, ReadingMode modeR)
                : base(slaveAddress, I2cBusSpeed.FastMode, I2cSharingMode.Shared)
        {
            m_Timer = new Timer(CheckState, null, Timeout.Infinite, Timeout.Infinite);

            this.m_ReadingMode = modeR;
            this.m_ReadInterval = readInterval;
        }

        #endregion Constructor


        protected void InitializeTimer() => this.Initialize(m_ReadingMode);

        private void Initialize(ReadingMode newMode)
        {
            this.m_ReadingMode = newMode;

            if (m_ReadingMode == ReadingMode.Continuous)
                m_Timer.Change(0, (int)m_ReadInterval.TotalMilliseconds);
            else
                m_Timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void CheckState(object state) => OnTimerAsync();

        protected abstract void OnTimerAsync();

        public virtual new void Dispose()
        {
            m_Timer.Dispose();
            base.Dispose();
        }
    }
}
