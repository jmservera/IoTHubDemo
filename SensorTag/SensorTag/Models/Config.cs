using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;

namespace SensorTag.Models
{
    /// <summary>
    /// A Configuration class, just fill the config.json file
    /// with the form:
    /// 
    /// {
    ///  "IotHubUri": "[hubname].azure-devices.net",
    ///  "DeviceName": "[registeredname]",
    ///  "DeviceKey": "[registeredkey]"
    /// }
    /// </summary>
    public class Config: BindableBase
    {
        string iotHubUri;

        public string IotHubUri
        {
            get
            {
                return iotHubUri;
            }

            set
            {
                Set(ref iotHubUri , value);
            }
        }
        string deviceName;

        public string DeviceName
        {
            get
            {
                return deviceName;
            }

            set
            {
                Set(ref deviceName , value);
            }
        }
        string deviceKey;

        public string DeviceKey
        {
            get
            {
                return deviceKey;
            }

            set
            {
                Set(ref deviceKey , value);
            }
        }
        string iotSuiteUri;

        public string IotSuiteUri
        {
            get
            {
                return iotSuiteUri;
            }

            set
            {
                Set(ref iotSuiteUri , value);
            }
        }
        string iotSuiteDeviceName;

        public string IotSuiteDeviceName
        {
            get
            {
                return iotSuiteDeviceName;
            }

            set
            {
                Set(ref iotSuiteDeviceName , value);
            }
        }
        string iotSuiteDeviceKey;

        public string IotSuiteDeviceKey
        {
            get
            {
                return iotSuiteDeviceKey;
            }

            set
            {
                Set(ref iotSuiteDeviceKey, value);
            }
        }
        string eventHubNamespace;

        public string EventHubNamespace
        {
            get
            {
                return eventHubNamespace;
            }

            set
            {
                Set(ref eventHubNamespace, value);
            }
        }
        string eventHubName;

        public string EventHubName
        {
            get
            {
                return eventHubName;
            }

            set
            {
                Set(ref eventHubName, value);
            }
        }
        string policyName;

        public string PolicyName
        {
            get
            {
                return policyName;
            }

            set
            {
                Set(ref policyName, value);
            }
        }
        string key;

        public string Key
        {
            get
            {
                return key;
            }

            set
            {
                Set(ref key, value);
            }
        }
        string partitionkey;

        public string Partitionkey
        {
            get
            {
                return partitionkey;
            }

            set
            {
                Set(ref partitionkey, value);
            }
        }

        static Config _config;
        public static Config Default
        {
            get
            {
                if (_config == null)
                {
                    _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
                }
                return _config;
            }
        }
    }
}
