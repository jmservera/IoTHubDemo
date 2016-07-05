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
using Windows.Devices.Geolocation;
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

        Geoposition position;

        DeviceClient ioTHubDeviceClient;
        CancellationTokenSource ioTHubReceiverToken;

        DeviceClient ioTSuiteDeviceClient;
        CancellationTokenSource ioTSuiteReceiverToken;

        Amqp.Session eventHubAmqpSession;
        Amqp.SenderLink senderlink;

        string eventHubName;
        string partitionKey;

        Timer valuesSender;
        SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public SensorValues SensorValues { get; } = new SensorValues();
        public string[] Modes { get; } = new string[] { "EventHub", "IoTHub", "IoTSuite" };

        string currentMode="IoTHub";
        public string CurrentMode
        {
            get { return currentMode; }
            set
            {
                currentMode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentMode)));
                connect();
            }
        }

        Timer simulatedTimer;
        double fakeInternalTemp=25, fakeExternalTemp=21, fakeHumidity=50, fakeLux=1000;
        Random r = new Random();
        bool simulated;

        private void simulateValuesInstance()
        {
            if (simulated)
            {
                nextValue(ref fakeInternalTemp, 0.3);
                nextValue(ref fakeExternalTemp, 0.3);
                nextValue(ref fakeHumidity, 2.5);
                if (fakeHumidity < 0) fakeHumidity = 0;
                else if (fakeHumidity > 100) fakeHumidity = 100;
                nextValue(ref fakeLux, 90);
                if (fakeLux < 0) fakeLux = 0;
                Tag_HumidityReceived(this, new DoubleEventArgs(fakeHumidity));
                Tag_TemperatureReceived(this, new DoubleEventArgs(fakeInternalTemp));
                Tag_IrTemperatureReceived(this, new DoubleEventArgs(fakeInternalTemp));
                Tag_IrAmbTemperatureReceived(this, new DoubleEventArgs(fakeExternalTemp));
                Tag_LuxReceived(this, new DoubleEventArgs(fakeLux));
            }
        }
        private static void simulateValues(object state)
        {
            ((MainPage)state).simulateValuesInstance();
        }

        private void nextValue(ref double variable, double delta)
        {
            var value = r.NextDouble();
            value -= 0.5;
            variable += delta*(value*2);
        }

        public bool Simulated
        {
            get { return simulated; }
            set
            {
                simulated = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Simulated)));
                simulatedTimer.Change(simulated ? 0 : Timeout.Infinite, 1000);
            }
        }


        private async void connect()
        {
            Logger.Log($"Connecting to {currentMode}", LogLevel.Warning);
            try
            {
                if (eventHubAmqpSession != null)
                {
                    senderlink.Close();
                    senderlink = null;
                    eventHubAmqpSession.Close();
                    eventHubAmqpSession = null;
                }
                if (ioTHubDeviceClient != null)
                {
                    await ioTHubDeviceClient.CloseAsync();
                    ioTHubDeviceClient = null;
                    if (ioTHubReceiverToken != null)
                    {
                        ioTHubReceiverToken.Cancel();
                        ioTHubReceiverToken = null;
                    }
                }
                if (ioTSuiteDeviceClient != null)
                {
                    await ioTSuiteDeviceClient.CloseAsync();
                    ioTSuiteDeviceClient = null;
                    if (ioTSuiteReceiverToken != null)
                    {
                        ioTSuiteReceiverToken.Cancel();
                        ioTSuiteReceiverToken = null;
                    }
                }

                switch (CurrentMode)
                {
                    case "EventHub":
                        {
                            var config = Config.Default;
                            string eventHubNamespace = config.EventHubNamespace;
                            eventHubName = config.EventHubName;
                            string policyName = config.PolicyName;
                            string key = config.Key;
                            partitionKey = config.Partitionkey;

                            Amqp.Address address = new Amqp.Address(
                                string.Format("{0}.servicebus.windows.net", eventHubNamespace),
                                5671, policyName, key);
                            await Task.Run(() =>
                            {
                                Amqp.Connection connection = new Amqp.Connection(address);
                                eventHubAmqpSession = new Amqp.Session(connection);
                                senderlink = new Amqp.SenderLink(eventHubAmqpSession,
    string.Format("send-link:{0}", eventHubName), eventHubName);
                            });
                        }
                        break;
                    case "IoTHub":
                        {
                            var key = AuthenticationMethodFactory.CreateAuthenticationWithRegistrySymmetricKey(Config.Default.DeviceName, Config.Default.DeviceKey);
                            ioTHubDeviceClient = DeviceClient.Create(Config.Default.IotHubUri, key, TransportType.Http1);

                            ioTHubReceiverToken = new CancellationTokenSource();
                            startMessageReceiver(ioTHubDeviceClient, ioTHubReceiverToken.Token);
                        }
                        break;
                    case "IoTSuite":
                        {
                            var key = AuthenticationMethodFactory.CreateAuthenticationWithRegistrySymmetricKey(Config.Default.IotSuiteDeviceName, Config.Default.IotSuiteDeviceKey);
                            ioTSuiteDeviceClient = DeviceClient.Create(Config.Default.IotSuiteUri, key, TransportType.Http1);

                            ioTSuiteReceiverToken = new CancellationTokenSource();
                            startMessageReceiver(ioTSuiteDeviceClient, ioTSuiteReceiverToken.Token);

                            //initialize

                            //{
                            //  "ObjectType":"DeviceInfo",
                            //  "Version":"1.0",
                            //  "IsSimulatedDevice":false,
                            //  "DeviceProperties":
                            //  {
                            //    "DeviceID":"mydevice01", "HubEnabledState":true
                            //  }, 
                            //  "Commands":
                            //  [
                            //    {"Name":"SetHumidity", "Parameters":[{"Name":"humidity","Type":"double"}]},
                            //    { "Name":"SetTemperature", "Parameters":[{"Name":"temperature","Type":"double"}]}
                            //  ]
                            //}
                            //{"ObjectType":"DeviceInfo","Version":"1.0","IsSimulatedDevice":false,"DeviceProperties":{"DeviceId":"884b85c2-a857-42d7-afdc-12a96328beec","HubEnabledState":1,"DeviceState":"normal","Manufacturer":"Juanma","Latitude":47.617025,"Longitude":-122.191285},"Commands":[{"Name":"SetHumidity","Parameters":[{"Name":"humidity","Type":"double"}]}]}
                            var position = await GetPositionAsync();

                            var data =JsonConvert.SerializeObject(new
                            {
                                ObjectType = "DeviceInfo",
                                Version = "1.0",
                                IsSimulatedDevice = 0,
                                DeviceProperties = new
                                {
                                    DeviceID = Config.Default.IotSuiteDeviceName,
                                    HubEnabledState = 1,
                                    DeviceState ="normal",
                                    Manufacturer="Juanma",
                                    Latitude= position?.Coordinate.Latitude,
                                    Longitude = position?.Coordinate.Longitude,
                                    Platform="csharp"
                                },
                                Commands = new[] {
                                    new {Name="SetHumidity", Parameters= new[] { new {Name="humidity",Type="double" } } }
                                }
                            });
                            var message = new Message(Encoding.UTF8.GetBytes(data));
                            await ioTSuiteDeviceClient.SendEventAsync(message);
                        }
                        break;
                }
                Logger.Log($"Connected to {currentMode}", LogLevel.Warning);

            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }



        int sends;
        int valueWrites;
        public int Sends { get { return sends; } }
        async void incrementSends()
        {
            sends++;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Sends")));
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
            simulatedTimer=new Timer(simulateValues, this, Timeout.Infinite, 1000);
            init();

            Logger.LogReceived += log;
            connect();
        }

        private async void startMessageReceiver(DeviceClient deviceClient, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var message = await deviceClient.ReceiveAsync();
                if (message != null) 
                {
                    var jsonMessage = Encoding.UTF8.GetString(message.GetBytes());
                    Logger.Log($"Message received: {jsonMessage}", LogLevel.Event);
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
                        if (CurrentMode == "IoTSuite")
                        {
                            //{"DeviceId":"mydevice01", "Temperature":50, "Humidity":50, "ExternalTemperature":55}
                            data = JsonConvert.SerializeObject(new
                            {
                                DeviceID = Config.Default.IotSuiteDeviceName,
                                ObjectTemperature = Math.Round(SensorValues.IrObject,2),
                                Humidity = Math.Round(SensorValues.Humidity,2),
                                Temperature=Math.Round(SensorValues.Temperature,2),
                                ExternalTemperature = Math.Round(SensorValues.IrWorld,2),
                                Lux= SensorValues.Lux
                            });
                        }
                        else
                        {
                            data = JsonConvert.SerializeObject(sensorInfoList);
                        }
                        count = sensorInfoList.Count;
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
                switch (CurrentMode)
                {
                    case "EventHub":
                        {
                            if (eventHubAmqpSession != null)
                            {


                                Amqp.Message message = new Amqp.Message()
                                {
                                    BodySection = new Amqp.Framing.Data()
                                    {
                                        Binary = System.Text.Encoding.UTF8.GetBytes(data)
                                    }
                                };

                                message.MessageAnnotations = new Amqp.Framing.MessageAnnotations();
                                message.MessageAnnotations[new Amqp.Types.Symbol("x-opt-partition-key")] =
                                   string.Format("pk:", partitionKey);
                                await Task.Run(() => senderlink.Send(message));
                            }
                            else
                            {
                                Logger.LogError("EventHub Not initialized");
                            }
                        }
                        break;
                    case "IoTHub":
                        {
                            if (ioTHubDeviceClient != null)
                            {
                                var message = new Message(Encoding.UTF8.GetBytes(data));
                                await ioTHubDeviceClient.SendEventAsync(message);
                            }
                            else
                            {
                                Logger.LogError("IoT Hub Not initialized");
                            }
                        }
                        break;
                    case "IoTSuite":
                        {
                            if (ioTSuiteDeviceClient != null)
                            {
                                var message = new Message(Encoding.UTF8.GetBytes(data));
                                await ioTSuiteDeviceClient.SendEventAsync(message);
                            }
                            else
                            {
                                Logger.LogError("IoT Suite Not initialized");
                            }
                        }
                        break;
                }
                Logger.LogInfo($"Sent {count} values as a single message to {currentMode}");
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

        private async Task<Geoposition> GetPositionAsync()
        {
            // Request permission to access location
            var accessStatus = await Geolocator.RequestAccessAsync();

            if (accessStatus == GeolocationAccessStatus.Allowed)
            {
                Geolocator geolocator = new Geolocator();

                // Carry out the operation
                return await geolocator.GetGeopositionAsync();
            }
            else
                return null;
        }
    }
}
