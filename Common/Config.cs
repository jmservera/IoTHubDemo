using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
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
    public class Config
    {
        public string IotHubUri { get; set; }
        public string DeviceName { get; set; }
        public string DeviceKey { get; set; }

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
