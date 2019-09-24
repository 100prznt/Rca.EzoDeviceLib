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
    public abstract class EzoTimedBase : EzoBase
    {
        private readonly Timer timer;

        private ReadingMode readingMode;
        public ReadingMode ReadingMode
        {
            get { return readingMode; }
            set { this.Initialize(value); }
        }

        private TimeSpan readInterval;
        public TimeSpan ReadInterval
        {
            get { return readInterval; }
            set
            {
                if (ReadingMode == ReadingMode.Manual)
                    throw new NotSupportedException($"You cannot change {nameof(ReadInterval)} when {nameof(ReadingMode)} is set to {ReadingMode.Manual}.");

                readInterval = value;
                timer?.Change(0, (int)readInterval.TotalMilliseconds);
            }
        }

        public EzoTimedBase(byte slaveAddress, TimeSpan readInterval, ReadingMode modeR)
                : base(slaveAddress, I2cBusSpeed.FastMode, I2cSharingMode.Shared)
        {
            timer = new Timer(CheckState, null, Timeout.Infinite, Timeout.Infinite);

            this.readingMode = modeR;
            this.readInterval = readInterval;
        }

        protected void InitializeTimer() => this.Initialize(readingMode);

        private void Initialize(ReadingMode newMode)
        {
            this.readingMode = newMode;

            if (readingMode == ReadingMode.Continuous)
                timer.Change(0, (int)readInterval.TotalMilliseconds);
            else
                timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void CheckState(object state) => OnTimerAsync();

        protected abstract void OnTimerAsync();

        public virtual new void Dispose()
        {
            timer.Dispose();
            base.Dispose();
        }
    }
}
