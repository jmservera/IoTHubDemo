using System;

namespace SensorTag
{
    public class DoubleEventArgs : EventArgs
    {
        public double Value { get; private set; }
        public DoubleEventArgs(double value)
        {
            Value = value;
        }
    }
}
