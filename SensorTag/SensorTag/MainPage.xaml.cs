using Common;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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
    public sealed partial class MainPage : Page, INotifyPropertyChanged
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

        
        int sends;
        int valueWrites;
        public int Sends { get { return sends; } }
        async void incrementSends()
        {
            sends++;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Sends")));
        }

        private bool eventHubs;

        public bool EventHubs
        {
            get { return eventHubs; }
            set
            {
                eventHubs = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EventHubs)));
            }
        }

        private bool sendEnabled;

        public bool SendEnabled
        {
            get { return sendEnabled; }
            set {
                sendEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SendEnabled)));
            }
        }



        public int ValueWrites { get { return valueWrites; }}
        async void incrementWrites()
        {
            valueWrites++;

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ValueWrites")));
        }

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
            Logger.Log("Tag Connected, reading values", LogLevel.Event);
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

        public event PropertyChangedEventHandler PropertyChanged;

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
                    incrementWrites();
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
            int count = 0;
            string data = null;
            if (Monitor.TryEnter(sensorInfoList))
            {
                try
                {
                    if (sensorInfoList.Count > 0)
                    {
                        count = sensorInfoList.Count;
                        data = JsonConvert.SerializeObject(sensorInfoList);
                        sensorInfoList.Clear();
                       
                    }
                }
                finally
                {
                    Monitor.Exit(sensorInfoList);
                }
                try
                {
                    if (data != null)
                    {
                        await sendMessage(data, count);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
            }
        }

        private async Task sendMessage(string data, int count)
        {
            if (SendEnabled)
            {
                if (eventHubs)
                {
                    var config = Config.Default;
                    string eventHubNamespace = config.EventHubNamespace;
                    string eventHubName = config.EventHubName;
                    string policyName = config.PolicyName;
                    string key = config.Key;
                    string partitionkey = config.Partitionkey;

                    Amqp.Address address = new Amqp.Address(
                        string.Format("{0}.servicebus.windows.net", eventHubNamespace),
                        5671, policyName, key);

                    Amqp.Connection connection = new Amqp.Connection(address);
                    Amqp.Session session = new Amqp.Session(connection);
                    Amqp.SenderLink senderlink = new Amqp.SenderLink(session,
                        string.Format("send-link:{0}", eventHubName), eventHubName);

                    Amqp.Message message = new Amqp.Message()
                    {
                        BodySection = new Amqp.Framing.Data()
                        {
                            Binary = System.Text.Encoding.UTF8.GetBytes(data)
                        }
                    };

                    message.MessageAnnotations = new Amqp.Framing.MessageAnnotations();
                    message.MessageAnnotations[new Amqp.Types.Symbol("x-opt-partition-key")] =
                       string.Format("pk:", partitionkey);
                    await Task.Run(() => senderlink.Send(message));
                }
                else
                {
                    var message = new Message(Encoding.UTF8.GetBytes(data));
                    await deviceClient.SendEventAsync(message);
                    Logger.LogInfo($"Sent {count} values as a single message");
                }
                incrementSends();
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
