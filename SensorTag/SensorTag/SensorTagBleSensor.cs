using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace SensorTag
{
    public class SensorTagBleSensor
    {
        public GattCharacteristic Data { get; set; }
        public GattCharacteristic Configuration { get; set; }
        public GattCharacteristic Period { get; set; }
        public Guid Id { get; private set; }

        public GattDeviceService Service { get; private set; }
        public async Task EnableNotifications()
        {
            await Data.WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.Notify);
            Data.ValueChanged += getData;

            using (var writer = new DataWriter())
            {
                // Special value for Gyroscope to enable all 3 axes
                //if (sensor == GYROSCOPE)
                //    writer.WriteByte((Byte)0x07);
                //else
                writer.WriteByte((Byte)0x01);
                await Configuration.WriteValueAsync(writer.DetachBuffer());
            }
        }

        public event Action<GattCharacteristic, GattValueChangedEventArgs> DataReceived;

        private void getData(GattCharacteristic sender, GattValueChangedEventArgs args)=> DataReceived?.Invoke(sender, args);


        public SensorTagBleSensor(Guid id, GattDeviceService service)
        {
            Id = id;
            Service = service;
        }
    }
}
