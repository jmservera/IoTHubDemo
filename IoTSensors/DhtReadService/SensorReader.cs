using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices;
using Windows.Devices.Gpio;

namespace DhtReadService
{
    public sealed class SensorReader
    {
        string statusText=string.Empty;
        Dht11 dht11=new Dht11();

        public SensorReader(int pinNumber)
        {
            if (LightningProvider.IsLightningEnabled)
            {
                // Set Lightning as the default provider
                LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();
                //GpioStatus.Text = "GPIO Using Lightning Provider";
            }
            else
            {
                //GpioStatus.Text = "GPIO Using Default Provider";
            }
            GpioController controller = GpioController.GetDefault();
            if (controller == null)
            {
                statusText = "GPIO is not available on this system";
            }
            else {
                GpioPin pin;
                try
                {
                    pin = controller.OpenPin(pinNumber);
                    dht11.Init(pin);
                    statusText = "Status: Initialized Successfully";
                }
                catch (Exception ex)
                {
                    statusText = "Failed to open GPIO pin: " + ex.Message;
                }
            }
        }

        public string Status
        {
            get { return statusText; }
        }

        public Dht11Reading Read()
        {
            Dht11Reading reading=null;
            int retryCount = 0;
            bool failed;
            do
            {
                failed = false;
                try {
                    reading = dht11.Sample();
                    reading.Failures = retryCount;
                }
                catch(TimeoutException tex)
                {
                    statusText = $"Timed out waiting for sample. {tex.Message}";
                    failed = true;
                }
                catch (InvalidOperationException)
                {
                    statusText = "Checksum validation failed";
                    failed = true;
                }
                catch
                {
                    statusText = "Could not read data";
                    failed = true;
                }
            } while (failed && (++retryCount < 20));

            if (!failed)
            {
                statusText = $"Succeeded ({retryCount} retries)";
            }

            System.Diagnostics.Debug.WriteLine(statusText);
            return reading;
        }
    }
}
