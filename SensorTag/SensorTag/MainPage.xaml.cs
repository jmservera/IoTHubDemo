using Common;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
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
        const string GUID = "ECD59A6D-1D0E-4CE2-A839-31167815A22D"; //todo create a unique guid per device
        const string GUIDir = "ECD59A6D-1D0E-4CE2-A839-31167815A22E"; //todo create a unique guid per device
        const string GUIDAMB = "ECD59A6D-1D0E-4CE2-A839-31167815A2F"; //todo create a unique guid per device

        const string ORGANIZATION = "Microsoft";
        const string DISPLAYNAME = "SensorTag 2650";
        const string LOCATION = "Madrid"; //todo config the location
        const string TEMPMEASURE = "Temperature";
        const string HUMIDMEASURE = "Humidity";
        const string TEMPUNITS = "C";
        const string HUMIDUNITS = "%";

        DeviceClient deviceClient;

        public MainPage()
        {
            this.InitializeComponent();
            var key = AuthenticationMethodFactory.CreateAuthenticationWithRegistrySymmetricKey(Config.Default.DeviceName, Config.Default.DeviceKey);
            deviceClient = DeviceClient.Create(Config.Default.IotHubUri, key, TransportType.Http1);
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
            tag.IrAmbTemperatureReceived += Tag_IrAmbTemperatureReceived;
            tag.IrTemperatureReceived += Tag_IrTemperatureReceived;
        }

        private async void Tag_IrTemperatureReceived(object sender, DoubleEventArgs e)
        {
            await sendValue(e, GUIDir, ORGANIZATION, DISPLAYNAME + "Ir", LOCATION, TEMPMEASURE, TEMPUNITS);
        }

        private async void Tag_IrAmbTemperatureReceived(object sender, DoubleEventArgs e)
        {
            await sendValue(e, GUIDAMB, ORGANIZATION, DISPLAYNAME + "Amb Ir", LOCATION, TEMPMEASURE, TEMPUNITS);
        }

        private async void Tag_TemperatureReceived(object sender, DoubleEventArgs e)
        {
            await sendValue(e, GUID, ORGANIZATION, DISPLAYNAME, LOCATION, TEMPMEASURE, TEMPUNITS);
        }

        private async void Tag_HumidityReceived(object sender, DoubleEventArgs e)
        {
            await sendValue(e, GUID,ORGANIZATION,DISPLAYNAME,LOCATION,HUMIDMEASURE,HUMIDUNITS);
        }

        private async Task sendValue(DoubleEventArgs e, string guid, string org, string display, string location, string measure, string units)
        {
            try
            {
                log($"{display} {measure}:{e.Value} Time:{DateTime.Now}");

                var info = new SensorInfo
                {
                    Guid = guid,
                    Organization = org,
                    DisplayName = display,
                    Location = location,
                    MeasureName = measure,
                    UnitOfMeasure = units,
                    Value = e.Value,
                    TimeCreated = DateTime.UtcNow
                };
                string dataBuffer = JsonConvert.SerializeObject(info);
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
