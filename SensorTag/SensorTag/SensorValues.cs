using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Template10.Mvvm;
using Windows.UI.Core;

namespace SensorTag
{
    public class SensorValues : BindableBase
    {
        public SensorValues()
        {
        }

        private double lux;

        public double Lux
        {
            get { return lux; }
            set { Set(ref lux, value);}
        }


        private double irWorld;

        public double IrWorld
        {
            get { return irWorld; }
            set { Set(ref irWorld, value); }
        }

        private double irObject;

        public double IrObject
        {
            get { return irObject; }
            set { Set(ref irObject, value); }
        }

        private double temperature;

        public double Temperature
        {
            get { return temperature; }
            set { Set(ref temperature, value); }
        }

        private double humidity;

        public double Humidity
        {
            get { return humidity; }
            set { Set(ref humidity, value); }
        }
    }
}
