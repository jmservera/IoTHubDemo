using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using DhtReadService;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace ReadService
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral d;

        const string iotHubUri = "jmHub.azure-devices.net";
        const string deviceKey = "ou7pkZF5sNxdlgy8zUcMa8U3iO+mi54LwvQ8kmKdVdc=";
        const string rpiName = "myRpi2";

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            try {
                d = taskInstance.GetDeferral();

                var key = AuthenticationMethodFactory.CreateAuthenticationWithRegistrySymmetricKey(rpiName, deviceKey);
                DeviceClient deviceClient = DeviceClient.Create(iotHubUri, key, TransportType.Http1); 

                Task ts = SendEvents(deviceClient);
                Task tr = ReceiveCommands(deviceClient);

                await Task.WhenAll(ts, tr);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                d.Complete();
            }
        }

        private async Task ReceiveCommands(DeviceClient deviceClient)
        {
            System.Diagnostics.Debug.WriteLine("\nDevice waiting for commands from IoTHub...\n");
            Message receivedMessage;
            string messageData;
            int recoverTimeout=1000;
            while (true)
            {
                try
                {
                    receivedMessage = await deviceClient.ReceiveAsync();// TimeSpan.FromSeconds(1));

                    if (receivedMessage != null)
                    {
                        messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                        System.Diagnostics.Debug.WriteLine(String.Format("\t{0}> Received message: {1}", DateTime.Now.ToLocalTime(), messageData));
                        await deviceClient.CompleteAsync(receivedMessage);
                    }
                    recoverTimeout = 1000;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    await Task.Delay(recoverTimeout);
                    recoverTimeout *= 10; // increment timeout for connection recovery
                    if(recoverTimeout>600000)//set a maximum timeout
                    {
                        recoverTimeout = 600000;
                    }
                }
            }

        }

        const string GUID = "58980B74-8117-464A-A9FA-1850B0E2F0B3"; //todo create a unique guid per device
        const string ORGANIZATION = "Microsoft";
        const string DISPLAYNAME = "Raspberry Pi 2 DHT22";
        const string LOCATION = "Madrid"; //todo config the location
        const string TEMPMEASURE = "Temperature";
        const string HUMIDMEASURE = "Humidity";
        const string TEMPUNITS = "C";
        const string HUMIDUNITS = "%";
        const string jsonFormat = "{{\"guid\":\"{0}\", \"organization\":\"{1}\", \"displayname\": \"{2}\", \"location\": \"{3}\", \"measurename\": \"{4}\", \"unitofmeasure\": \"{5}\", \"value\":{6} }}";
        

        private async Task SendEvents(DeviceClient deviceClient)
        {
            try
            {
                string dataBuffer;
                SensorReader c = new SensorReader(4);
                Guid id = Guid.NewGuid();
                while (true)
                {
                    var data=c.Read();
                    if (data!=null && data.IsValid)
                    {
                        try {
                            System.Diagnostics.Debug.WriteLine(string.Format("Temp:{0} Hum:{1}", data.Temperature, data.Humidity));
                            dataBuffer = string.Format(jsonFormat,
                                GUID, ORGANIZATION, DISPLAYNAME, LOCATION, TEMPMEASURE, TEMPUNITS, data.Temperature);
Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
                            await deviceClient.SendEventAsync(eventMessage);
                            dataBuffer = string.Format(jsonFormat,
                                GUID, ORGANIZATION, DISPLAYNAME, LOCATION, HUMIDMEASURE, HUMIDUNITS, data.Humidity);
                            eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
                            await deviceClient.SendEventAsync(eventMessage);
                        }
                        catch(Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                        }
                    }
                    await Task.Delay(2000).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0}: {1}", ex.Message, ex.StackTrace));
            }
        }
    }
}
