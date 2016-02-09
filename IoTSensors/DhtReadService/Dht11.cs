using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace DhtReadService
{
    public sealed class Dht11
    {
        const int sampleHoldLowMillis = 10;
        // This is the threshold used to determine whether a bit is a '0' or a '1'.
        // A '0' has a pulse time of 76 microseconds, while a '1' has a
        // pulse time of 120 microseconds. 110 is chosen as a reasonable threshold.
        const uint oneThresholdMicroseconds = 110;

        const uint sampleTimeoutMillis = 10;
        const uint initialRisingEdgeTimeoutMillis = 1;

        GpioPin pin;
        GpioPinDriveMode inputDriveMode = GpioPinDriveMode.Input;

        public GpioPinDriveMode InputDriveMode
        {
            get
            {
                return inputDriveMode;
            }

            set
            {
                inputDriveMode = value;
            }
        }

        public Dht11()
        {

        }

        public void Init(GpioPin gpioPin)
        {
            // Use InputPullUp if supported, otherwise fall back to Input (floating)
            inputDriveMode =
                gpioPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp) ?
                GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Input;

            gpioPin.SetDriveMode(inputDriveMode);
            pin = gpioPin;
        }

        public Dht11Reading Sample()
        {
            BitVector bits = new BitVector(40);

            pin.SetDriveMode(GpioPinDriveMode.Output);
            // Latch low value onto pin
            pin.Write(GpioPinValue.Low);

            // Wait for at least 18 ms
            Task.Delay(sampleHoldLowMillis).Wait();
            pin.Write(GpioPinValue.High);
            Task.Delay(TimeSpan.FromTicks(200)).Wait();
            var stopwatch = Stopwatch.StartNew();
            // Set pin back to input
            pin.SetDriveMode(inputDriveMode);
            GpioPinValue previousValue = pin.Read();
            // catch the first rising edge
            for (;;)
            {
                if (stopwatch.ElapsedMilliseconds > initialRisingEdgeTimeoutMillis)
                {
                    throw new TimeoutException("Initial Rising Timeout Exception");
                }

                GpioPinValue value = pin.Read();
                if (value != previousValue)
                {
                    // rising edge?
                    if (value == GpioPinValue.High)
                    {
                        break;
                    }
                    previousValue = value;
                }
            }

            long prevTime = 0;


            stopwatch.Restart();
            // capture every falling edge until all bits are received or
            // timeout occurs
            for (uint i = 0; i < (bits.Length + 1);)
            {
                if (stopwatch.ElapsedMilliseconds > sampleTimeoutMillis)
                {
                    throw new TimeoutException("Bits reading Timeout Exception");
                }

                GpioPinValue value = pin.Read();
                if ((previousValue == GpioPinValue.High) && (value == GpioPinValue.Low))
                {
                    // A falling edge was detected

                    // Calculate the microseconds in a fractional second
                    long now = (long)(stopwatch.Elapsed.TotalSeconds * 1000000);

                    if (i != 0)
                    {
                        var difference = now - prevTime;
                        bits[bits.Length - i] =
                            difference > oneThresholdMicroseconds;
                    }

                    prevTime = now;
                    ++i;
                }

                previousValue = value;
            }

            var reading = new Dht11Reading(bits.ToULongValues()[0]);
            if (!reading.IsValid)
            {
                // checksum mismatch
                throw new InvalidOperationException("invalid checksum");
            }

            return reading;
        }

        public bool PullResistorRequired
        {
            get
            {
                return inputDriveMode != GpioPinDriveMode.InputPullUp;
            }
        }
    }
}