using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using X2CodingLab.SensorTag.Sensors;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SensorTag
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            //var selector=Windows.Devices.Bluetooth.BluetoothDevice.GetDeviceSelector();
            //System.Diagnostics.Debug.WriteLine(selector);
            //Windows.Devices.Bluetooth.BluetoothLEDevice.
            //startSensor();

            var tag = new SensorTag();
            tag.Init();

            this.Unloaded += MainPage_Unloaded;
        }

        private async void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if(acc!= null)
            {
                await acc.DisableNotifications();
                await acc.DisableSensor();
            }
        }

        Accelerometer acc;
        private async void startSensor()
        {
            acc = new Accelerometer();
            var info = await Windows.Devices.Enumeration.DeviceInformation.CreateFromIdAsync("0000180a-0000-1000-8000-00805f9b34fb");
            await acc.Initialize();
            await acc.EnableSensor();
            await acc.EnableNotifications();
            acc.SensorValueChanged += Acc_SensorValueChanged;

        }

        private void Acc_SensorValueChanged(object sender, X2CodingLab.SensorTag.SensorValueChangedEventArgs e)
        {
            System.Diagnostics.Debug.Write(e.RawData);
        }
    }
}
