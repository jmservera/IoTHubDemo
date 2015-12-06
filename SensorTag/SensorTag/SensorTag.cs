using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace SensorTag
{
    //https://github.com/sandeepmistry/node-sensortag/blob/master/lib/cc2650.js

    public class SensorTag
    {

        //BTHLEDevice\{00001800-0000-1000-8000-00805f9b34fb}_Dev_VID&01000d_PID&0000_REV&0110_b0b448c09803\8&3766f1f7&0&0001
        //BTHLEDevice\{00001801-0000-1000-8000-00805f9b34fb}_Dev_VID&01000d_PID&0000_REV&0110_b0b448c09803\8&3766f1f7&0&0008
        //BTHLEDevice\{0000180a-0000-1000-8000-00805f9b34fb}_Dev_VID&01000d_PID&0000_REV&0110_b0b448c09803\8&3766f1f7&0&000c
        //BTHLEDevice\{f000aa00-0451-4000-b000-000000000000}_Dev_VID&01000d_PID&0000_REV&0110_b0b448c09803\8&3766f1f7&0&001f
        //BTHLEDevice\{f000aa20-0451-4000-b000-000000000000}_Dev_VID&01000d_PID&0000_REV&0110_b0b448c09803\8&3766f1f7&0&0027
        //BTHLEDevice\{f000aa40-0451-4000-b000-000000000000}_Dev_VID&01000d_PID&0000_REV&0110_b0b448c09803\8&3766f1f7&0&002f
        //BTHLEDevice\{f000aa80-0451-4000-b000-000000000000}_Dev_VID&01000d_PID&0000_REV&0110_b0b448c09803\8&3766f1f7&0&0037
        //BTHLEDevice\{f000aa70-0451-4000-b000-000000000000}_Dev_VID&01000d_PID&0000_REV&0110_b0b448c09803\8&3766f1f7&0&003f
        //BTHLEDevice\{0000ffe0-0000-1000-8000-00805f9b34fb}_Dev_VID&01000d_PID&0000_REV&0110_b0b448c09803\8&3766f1f7&0&0047
        //BTHLEDevice\{f000aa64-0451-4000-b000-000000000000}_Dev_VID&01000d_PID&0000_REV&0110_b0b448c09803\8&3766f1f7&0&004c
        //BTHLEDevice\{f000ac00-0451-4000-b000-000000000000}_Dev_VID&01000d_PID&0000_REV&0110_b0b448c09803\8&3766f1f7&0&0051
        //BTHLEDevice\{f000ccc0-0451-4000-b000-000000000000}_Dev_VID&01000d_PID&0000_REV&0110_b0b448c09803\8&3766f1f7&0&0059
        //BTHLEDevice\{f000ffc0-0451-4000-b000-000000000000}_Dev_VID&01000d_PID&0000_REV&0110_b0b448c09803\8&3766f1f7&0&0061

        //string[] uuids = new string[] { "00001800-0000-1000-8000-00805f9b34fb",
        //    "00001801-0000-1000-8000-00805f9b34fb",
        //    "0000180a-0000-1000-8000-00805f9b34fb",
        //    "f000aa00-0451-4000-b000-000000000000",
        //    "f000aa20-0451-4000-b000-000000000000",
        //    "f000aa40-0451-4000-b000-000000000000",
        //    "f000aa80-0451-4000-b000-000000000000",
        //    "f000aa70-0451-4000-b000-000000000000",
        //    "0000ffe0-0000-1000-8000-00805f9b34fb",
        //    "f000aa64-0451-4000-b000-000000000000",
        //    "f000ac00-0451-4000-b000-000000000000",
        //    "f000ccc0-0451-4000-b000-000000000000",
        //    "f000ffc0-0451-4000-b000-000000000000" };

        const string TMP007_UUID = "F000AA00-0451-4000-B000-000000000000";
        const string MPU9250_UUID = "f000aa8004514000b000000000000000";
        const string BAROMETRIC_PRESSURE_UUID = "f000aa4004514000b000000000000000";
        const string IO_UUID = "f000aa6404514000b000000000000000";
        const string LUXOMETER_UUID = "f000aa7004514000b000000000000000";

        const string BAROMETRIC_PRESSURE_CONFIG_UUID = "f000aa4204514000b000000000000000";

        const string MPU9250_CONFIG_UUID = "f000aa8204514000b000000000000000";
        const string MPU9250_DATA_UUID = "f000aa8104514000b000000000000000";
        const string MPU9250_PERIOD_UUID = "f000aa8304514000b000000000000000";

        uint MPU9250_GYROSCOPE_MASK = 0x0007;
        uint MPU9250_ACCELEROMETER_MASK = 0x0038;
        uint MPU9250_MAGNETOMETER_MASK = 0x0040;

        const string IO_DATA_UUID = "f000aa6504514000b000000000000000";
        const string IO_CONFIG_UUID = "f000aa6604514000b000000000000000";

        const string LUXOMETER_CONFIG_UUID = "f000aa7204514000b000000000000000";
        const string LUXOMETER_DATA_UUID = "f000aa7104514000b000000000000000";
        const string LUXOMETER_PERIOD_UUID = "f000aa7304514000b000000000000000";

        List<GattDeviceService> serviceList = new List<GattDeviceService>();

        public async Task Init()
        {
            var uuid = new Guid(TMP007_UUID);
            var serv = await getService(uuid);
            serviceList.Add(serv);
            var chars=serv.GetAllCharacteristics();
            foreach (var characteristic in chars)
            {
                System.Diagnostics.Debug.WriteLine(characteristic.UserDescription);
                if (characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                {
                    await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    characteristic.ValueChanged += Characteristic_ValueChanged;
                }
                if (characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write))
                {
                    var writer = new Windows.Storage.Streams.DataWriter();
                    // Special value for Gyroscope to enable all 3 axes
                    //if (sensor == GYROSCOPE)
                    //    writer.WriteByte((Byte)0x07);
                    //else
                    writer.WriteByte((Byte)0x01);

                    await characteristic.WriteValueAsync(writer.DetachBuffer());
                }
            }
        }

        public void CloseAll()
        {
            foreach(var service in serviceList)
            {
                service.Dispose();
            }
        }
        private void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            byte[] bArray = new byte[args.CharacteristicValue.Length];
            DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(bArray);
            Int16 AmbTemp = (Int16)(((UInt16)bArray[3] << 8) + (UInt16)bArray[2]);

            Int16 temp = (Int16)(((UInt16)bArray[1] << 8) + (UInt16)bArray[0]);
            //double Vobj2 = (double)temp;
            //Vobj2 *= 0.00000015625;
            //double Tdie = AmbTemp + 273.15;

            //const double S0 = 5.593E-14;            // Calibration factor
            //const double a1 = 1.75E-3;
            //const double a2 = -1.678E-5;
            //const double b0 = -2.94E-5;
            //const double b1 = -5.7E-7;
            //const double b2 = 4.63E-9;
            //const double c2 = 13.4;
            //const double Tref = 298.15;

            //double S = S0 * (1 + a1 * (Tdie - Tref) + a2 * Math.Pow((Tdie - Tref), 2));
            //double Vos = b0 + b1 * (Tdie - Tref) + b2 * Math.Pow((Tdie - Tref), 2);
            //double fObj = (Vobj2 - Vos) + c2 * Math.Pow((Vobj2 - Vos), 2);
            //double tObj = Math.Pow(Math.Pow(Tdie, 4) + (fObj / S), 0.25);

            //tObj = ((tObj - 273.15) - 32)*5/9;
            const float SCALE_LSB = 0.03125f;
            float t,t2;
            int it;

            it = (int)((temp) >> 2);
            t = ((float)(it)) * SCALE_LSB;

            it = (int)((AmbTemp) >> 2);
            t2 = (float)it;
            t2=t2 * SCALE_LSB;
            System.Diagnostics.Debug.WriteLine($"Chip: {t2} IR: {t}");
            //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            //{
            //    AmbTempOut.Text = string.Format("Chip:\t{0:0.0####}", AmbTemp);
            //    ObjTempOut.Text = string.Format("IR:  \t{0:0.0####}", tObj);
            //});
        }

        private static async Task<GattDeviceService> getService(Guid uuid)
        {
            var services = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(uuid), null);
            if (services != null && services.Count > 0)
            {
                if (services[0].IsEnabled)
                {
                    GattDeviceService service = await GattDeviceService.FromIdAsync(services[0].Id);
                    await Task.Delay(500);
                    if (service.Device.ConnectionStatus == BluetoothConnectionStatus.Connected)
                    {
                        return service;
                    }
                }
            }

            return null;
        }
    }
}
