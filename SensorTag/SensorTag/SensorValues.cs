using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using GalaSoft.MvvmLight;
using Windows.UI.Core;

namespace SensorTag
{
    public class SensorValues : ObservableObject
    {
        CoreDispatcher dispatcher;

        public SensorValues()
        {
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
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

        protected async override void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!dispatcher.HasThreadAccess)
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    base.RaisePropertyChanged(propertyName);
                });
            }
            else {
                base.RaisePropertyChanged(propertyName);
            }
        }
        protected async override void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            if (!dispatcher.HasThreadAccess)
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    base.RaisePropertyChanged<T>(propertyExpression);
                });
            }
            else {
                base.RaisePropertyChanged<T>(propertyExpression);
            }
        }
    }
}
