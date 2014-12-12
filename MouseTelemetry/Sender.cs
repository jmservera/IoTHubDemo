using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MouseTelemetry
{
    
    public class Sender
    {
        EventHubClient _client;

        public Sender(string name)
        {
            _client = EventHubClient.Create(name);
        }

        public async void SendAsync(int x, int y)
        {
            try
            {
                var serializedXY = JsonConvert.SerializeObject(new { x = x, y = y, date=DateTime.UtcNow });
                var data = new EventData(UTF8Encoding.UTF8.GetBytes(serializedXY)) { PartitionKey = GetMacAddress() };
                await _client.SendAsync(data);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Ex: {0}\t{1}", ex.Message, ex.StackTrace);
            }
        }

        string macAddress = null;
        /// <summary>
        /// Finds the MAC address of the NIC with maximum speed.
        /// </summary>
        /// <returns>The MAC address.</returns>
        private string GetMacAddress()
        {
            if (macAddress == null)
            {
                const int MIN_MAC_ADDR_LENGTH = 12;
                long maxSpeed = -1;

                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    //log.Debug(
                    //    "Found MAC Address: " + nic.GetPhysicalAddress() +
                    //    " Type: " + nic.NetworkInterfaceType);

                    string tempMac = nic.GetPhysicalAddress().ToString();
                    if (nic.Speed > maxSpeed &&
                        !string.IsNullOrEmpty(tempMac) &&
                        tempMac.Length >= MIN_MAC_ADDR_LENGTH)
                    {
                        //log.Debug("New Max Speed = " + nic.Speed + ", MAC: " + tempMac);
                        maxSpeed = nic.Speed;
                        macAddress = tempMac;
                    }
                }
            }

            return macAddress;
        }

    }
}
