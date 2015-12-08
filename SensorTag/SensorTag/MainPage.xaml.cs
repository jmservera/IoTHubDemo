using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SensorTag
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const string iotHubUri = "[yourIoTHub].azure-devices.net";
        const string deviceName = "SensorTag";
        const string deviceKey = "[YourKey]";
        const string GUID = "[Create a GUID]"; //todo create a unique guid per device
        const string ORGANIZATION = "Microsoft";
        const string DISPLAYNAME = "SensorTag 2650";
        const string LOCATION = "Madrid"; //todo config the location
        const string TEMPMEASURE = "Temperature";
        const string HUMIDMEASURE = "Humidity";
        const string TEMPUNITS = "C";
        const string HUMIDUNITS = "%";
        const string jsonFormat = "{{\"guid\":\"{0}\", \"organization\":\"{1}\", \"displayname\": \"{2}\", \"location\": \"{3}\", \"measurename\": \"{4}\", \"unitofmeasure\": \"{5}\", \"value\":{6}, \"timecreated\":\"{7}\" }}";

        DeviceClient deviceClient;

        public MainPage()
        {
            this.InitializeComponent();
            var key = AuthenticationMethodFactory.CreateAuthenticationWithRegistrySymmetricKey(deviceName, deviceKey);
            deviceClient = DeviceClient.Create(iotHubUri, key, TransportType.Http1);
            init();
        }

        async void init()
        {
            var tag = new SensorTag();
            while (!tag.Connected)
            {
               await tag.Init();
                if (!tag.Connected)
                {
                    log("Tag not connected, retrying");
                    await Task.Delay(1000);
                }
            }
            tag.HumidityReceived += Tag_HumidityReceived;
            tag.TemperatureReceived += Tag_TemperatureReceived;
        }

        private async void Tag_TemperatureReceived(object sender, DoubleEventArgs e)
        {
            try
            {
                var timeCreated = $"{DateTime.UtcNow:u}".Replace(' ', 'T');
                log($"Temp:{e.Value} Time:{timeCreated}");

                string dataBuffer = string.Format(jsonFormat,
                    GUID, ORGANIZATION, DISPLAYNAME, LOCATION, TEMPMEASURE, TEMPUNITS, e.Value, timeCreated);
                Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
                await deviceClient.SendEventAsync(eventMessage);
            }
            catch (Exception ex)
            {
                log(ex.Message);
            }
        }

        private async void Tag_HumidityReceived(object sender, DoubleEventArgs e)
        {
            try
            {
                var timeCreated = $"{DateTime.UtcNow:u}".Replace(' ', 'T');
                log($"Hum:{e.Value} Time:{timeCreated}");
                var dataBuffer = string.Format(jsonFormat,
                    GUID, ORGANIZATION, DISPLAYNAME, LOCATION, HUMIDMEASURE, HUMIDUNITS, e.Value, timeCreated);
                var eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
                await deviceClient.SendEventAsync(eventMessage);
            }
            catch (Exception ex)
            {
                log(ex.Message);
            }
        }

        private async void log(string message)
        {
            Debug.WriteLine(message);
            await logger.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                logger.Text = message + Environment.NewLine + logger.Text;
                if (logger.Text.Length > 1000)
                {
                    logger.Text.Substring(0, 800);
                }
            });
        }
    }
}
