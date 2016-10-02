﻿using static Common.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace SensorTag
{
    //https://github.com/sandeepmistry/node-sensortag/blob/master/lib/cc2650.js
    //http://processors.wiki.ti.com/index.php/CC2650_SensorTag_User's_Guide
    public class SensorTag
    {        
        const string TMP007_UUID = "F000AA00-0451-4000-B000-000000000000";
        const string HDC1000_UUID = "F000AA20-0451-4000-B000-000000000000";
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
        Dictionary<string, SensorTagBleSensor> sensors = new Dictionary<string, SensorTagBleSensor>();

        public event EventHandler<DoubleEventArgs> HumidityReceived;
        public event EventHandler<DoubleEventArgs> TemperatureReceived;
        public event EventHandler<DoubleEventArgs> IrTemperatureReceived;
        public event EventHandler<DoubleEventArgs> IrAmbTemperatureReceived;
        public event EventHandler<DoubleEventArgs> LuxReceived;

        public bool Connected { get { return sensors.Count == 3; }  }

        public async Task Init()
        {
            if (!sensors.ContainsKey(HDC1000_UUID))
            {
                var humSensor = await getSensor(new Guid(HDC1000_UUID));
                if (humSensor != null)
                {
                    sensors.Add(HDC1000_UUID, humSensor);
                    await humSensor.EnableNotifications();
                    humSensor.DataReceived += HumSensor_DataReceived;
                    LogInfo("HDC Sensor connected.");
                }
                else
                {
                    LogError("HDC Sensor didn't connect, must retry.");
                }
            }

            if (!sensors.ContainsKey(TMP007_UUID))
            {
                var irSensor = await getSensor(new Guid(TMP007_UUID));
                if (irSensor != null)
                {
                    sensors.Add(TMP007_UUID, irSensor);
                    await irSensor.EnableNotifications();
                    irSensor.DataReceived += IrSensor_DataReceived;
                    LogInfo("IR Sensor connected.");
                }
                else
                {
                    LogError("Temp IR Sensor didn't connect, must retry.");
                }
            }

            if (!sensors.ContainsKey(LUXOMETER_UUID))
            {
                var luxSensor = await getSensor(new Guid(LUXOMETER_UUID));
                if (luxSensor != null)
                {
                    sensors.Add(LUXOMETER_UUID, luxSensor);
                    await luxSensor.EnableNotifications();
                    luxSensor.DataReceived += LuxSensor_DataReceived;
                    LogInfo("Lux Sensor connected.");
                }
                else
                {
                    LogError("Lux Sensor didn't connect, must retry.");
                }
            }
        }

        private void LuxSensor_DataReceived(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            byte[] bArray = getDataValue(args);
            ushort rawValue= (ushort)((bArray[1] << 8) + bArray[0]);
            ushort m = (ushort)(rawValue & 0x0FFF);
            ushort e = (ushort)((rawValue & 0xF000) >> 12);

            var value= m * (0.01 * Math.Pow(2.0, e));
            LuxReceived?.Invoke(this, new DoubleEventArgs(value));
        }

        private void IrSensor_DataReceived(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            byte[] bArray = getDataValue(args);
            ushort rawObjTemp = (ushort)((bArray[1] << 8) + bArray[0]);
            ushort rawAmbTemp = (ushort)((bArray[3] << 8) + bArray[2]);


            const float SCALE_LSB = 0.03125f;
            int it;

            it = (rawObjTemp) >> 2;
            var t = ((double)(it)) * SCALE_LSB;
            IrTemperatureReceived?.Invoke(this, new DoubleEventArgs(t));

            it = (rawAmbTemp) >> 2;
            var t2 = ((double)it) * SCALE_LSB;
            IrAmbTemperatureReceived?.Invoke(this, new DoubleEventArgs(t2));
        }

        private void HumSensor_DataReceived(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            byte[] bArray = getDataValue(args);
            UInt16 rawHum = (ushort)(((UInt16)bArray[3] << 8) + (UInt16)bArray[2]);

            Int16 rawTemp = (Int16)(((UInt16)bArray[1] << 8) + (UInt16)bArray[0]);

            //-- calculate temperature [°C]
            var temp = ((double)rawTemp / 65536) * 165 - 40;

            //-- calculate relative humidity [%RH]
            var hum = ((double)rawHum / 65536) * 100;

            System.Diagnostics.Debug.WriteLine($"temp: {temp} hum: {hum}");

            TemperatureReceived?.Invoke(this, new DoubleEventArgs(temp));
            HumidityReceived?.Invoke(this, new DoubleEventArgs(hum));
        }

        private static byte[] getDataValue(GattValueChangedEventArgs args)
        {
            byte[] bArray = new byte[args.CharacteristicValue.Length];
            using (var r = DataReader.FromBuffer(args.CharacteristicValue))
            {
                r.ReadBytes(bArray);
            }
            return bArray;
        }

        private async Task<SensorTagBleSensor> getSensor(Guid uuid)
        {
            var serv = await getService(uuid);
            if (serv != null)
            {
                var sensor = new SensorTagBleSensor(uuid, serv);
                serviceList.Add(serv);
                var chars = serv.GetAllCharacteristics();
                foreach (var characteristic in chars)
                {
                    if ((characteristic.Uuid.ToByteArray()[0] & (byte)3) == (byte)1)
                    {
                        sensor.Data = characteristic;
                    }
                    else
                    if ((characteristic.Uuid.ToByteArray()[0] & (byte)3) == (byte)2)
                    {
                        sensor.Configuration = characteristic;
                    }
                    else
                    if ((characteristic.Uuid.ToByteArray()[0] & (byte)3) == (byte)3)
                    {
                        sensor.Period = characteristic;
                    }
                }
                return sensor;
            }
            return null;
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
        }

        private static async Task<GattDeviceService> getService(Guid uuid)
        {
            var devices = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(uuid), null);
            if (devices != null && devices.Count > 0)
            {
                if (devices[0].IsEnabled)
                {
                    GattDeviceService deviceService = await GattDeviceService.FromIdAsync(devices[0].Id);
                    if (deviceService != null)
                    {
                        await Task.Delay(500);
                        if (deviceService.Device.ConnectionStatus == BluetoothConnectionStatus.Connected)
                        {
                            return deviceService;
                        }
                    }
                }
            }

            return null;
        }
    }
}
