using SensorTag.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;

namespace SensorTag.ViewModels
{

    public class ConfigViewModel : ViewModelBase
    {
        private Config config = Config.Default;
        public Config CurrentConfig
        {
            get { return config; }
            set { Set(ref config, value); }
        }
    }
}
