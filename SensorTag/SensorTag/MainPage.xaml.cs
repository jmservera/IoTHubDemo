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
using System.Threading;
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
        const string GUIDLux = "ECD59A6D-1D0E-4CE2-A839-31167815A30"; //todo create a unique guid per device

        const string ORGANIZATION = "Microsoft";
        const string DISPLAYNAME = "SensorTag 2650";
        const string LOCATION = "Madrid"; //todo config the location
        const string TEMPMEASURE = "Temperature";
        const string HUMIDMEASURE = "Humidity";
        const string LUXMEASURE = "Lux";
        const string TEMPUNITS = "C";
        const string HUMIDUNITS = "%";
        const string LUXUNITS = "%";

        DeviceClient deviceClient;
        Timer valuesSender;
        SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public SensorValues SensorValues { get; } = new SensorValues();

        public MainPage()
        {
            this.InitializeComponent();
            var key = AuthenticationMethodFactory.CreateAuthenticationWithRegistrySymmetricKey(Config.Default.DeviceName, Config.Default.DeviceKey);
            deviceClient = DeviceClient.Create(Config.Default.IotHubUri, key, TransportType.Http1);
            init();
            CancellationTokenSource t = new CancellationTokenSource();
            startMessageReceiver(t.Token);
            Logger.LogReceived += log;
        }

        private async void startMessageReceiver(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var message = await deviceClient.ReceiveAsync();
                if (message != null)
                {
                    var jsonMessage = Encoding.UTF8.GetString(message.GetBytes());
                    Logger.Log($"Message received: {jsonMessage}", LogLevel.Event);
                    //if (jsonMessage != null)
                    //{
                    //    nextStep(Steps.Lights);
                    //}
                    await deviceClient.CompleteAsync(message);
                }
            }
        }

        async void init()
        {
            valuesSender = new Timer(sendValues, null, 1000, 1000);
            var tag = new SensorTag();
            while (!tag.Connected)
            {
                try {
                    await tag.Init();
                    if (!tag.Connected)
                    {
                        Logger.LogError("Tag not connected, retrying");
                        await Task.Delay(1000);
                    }
                }
                catch(Exception ex)
                {
                    Logger.LogException(ex);
                    await Task.Delay(2000);
                }
            }

            tag.HumidityReceived += Tag_HumidityReceived;
            tag.TemperatureReceived += Tag_TemperatureReceived;
            tag.IrAmbTemperatureReceived += Tag_IrAmbTemperatureReceived;
            tag.IrTemperatureReceived += Tag_IrTemperatureReceived;
            tag.LuxReceived += Tag_LuxReceived;
        }

        private void Tag_LuxReceived(object sender, DoubleEventArgs e)
        {
            SensorValues.Lux = e.Value;
            sendValue(e, GUIDLux, ORGANIZATION, DISPLAYNAME + "Lux", LOCATION, LUXMEASURE, LUXUNITS);
        }

        private void Tag_IrTemperatureReceived(object sender, DoubleEventArgs e)
        {
            SensorValues.IrObject = e.Value;
            sendValue(e, GUIDir, ORGANIZATION, DISPLAYNAME + "Ir", LOCATION, TEMPMEASURE, TEMPUNITS);
        }

        private void Tag_IrAmbTemperatureReceived(object sender, DoubleEventArgs e)
        {
            SensorValues.IrWorld = e.Value;
            sendValue(e, GUIDAMB, ORGANIZATION, DISPLAYNAME + "Amb Ir", LOCATION, TEMPMEASURE, TEMPUNITS);
        }

        private void Tag_TemperatureReceived(object sender, DoubleEventArgs e)
        {
            SensorValues.Temperature = e.Value;
            sendValue(e, GUID, ORGANIZATION, DISPLAYNAME + "Temp", LOCATION, TEMPMEASURE, TEMPUNITS);
        }

        private void Tag_HumidityReceived(object sender, DoubleEventArgs e)
        {
            SensorValues.Humidity = e.Value;
            sendValue(e, GUID,ORGANIZATION, DISPLAYNAME + "Humidity", LOCATION,HUMIDMEASURE,HUMIDUNITS);
        }

        List<SensorInfo> sensorInfoList = new List<SensorInfo>();

        private void sendValue(DoubleEventArgs e, string guid, string org, string display, string location, string measure, string units)
        {
            try
            {
                Logger.LogInfo($"{display} {measure}:{e.Value} Time:{DateTime.Now}");

                Monitor.Enter(sensorInfoList);
                try
                {
                    sensorInfoList.Add(new SensorInfo
                    {
                        Guid = guid,
                        Organization = org,
                        DisplayName = display,
                        Location = location,
                        MeasureName = measure,
                        UnitOfMeasure = units,
                        Value = e.Value,
                        TimeCreated = DateTime.UtcNow
                    });
                }
                finally
                {
                    Monitor.Exit(sensorInfoList);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }


        private async void sendValues(object state)
        {
            Message message = null;
            int count = 0;
            if (Monitor.TryEnter(sensorInfoList))
            {
                try
                {
                    if (sensorInfoList.Count > 0)
                    {
                        count = sensorInfoList.Count;
                        var data = JsonConvert.SerializeObject(sensorInfoList);
                        message = new Message(Encoding.UTF8.GetBytes(data));
                        sensorInfoList.Clear();
                    }
                }
                finally
                {
                    Monitor.Exit(sensorInfoList);
                }
                try
                {
                    if (message != null)
                    {
                        await deviceClient.SendEventAsync(message);
                        Logger.LogInfo($"Sent {count} values as a single message");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
            }
        }

        private async void log(object sender, LoggerEventArgs eventArgs)
        {
            if (eventArgs.Level != LogLevel.Info)
            {
                await logger.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    logger.Text = eventArgs.Message + Environment.NewLine + logger.Text;
                    if (logger.Text.Length > 1000)
                    {
                        logger.Text.Substring(0, 800);
                    }
                });
            }
        }
    }
}
