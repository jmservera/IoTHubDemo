using Microsoft.ServiceBus.Messaging;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseTelemetry
{
    public partial class MainForm : Form
    {
        MouseHookListener _mouseListener;
        Sender _sender;
        PerformanceCounter cpuCounter;

        public MainForm()
        {
            InitializeComponent();

            startMouseTrackingButton.Enabled = true;
            stopMouseTrackingButton.Enabled = false;
            startCPUTrackingButton.Enabled = true;
            stopCpuTrackingButton.Enabled = false;


            _mouseListener = new MouseHookListener(new GlobalHooker());
            _mouseListener.MouseMove += listener_MouseMove;

            cpuCounter = new PerformanceCounter();
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";
            cpuCounter.NextValue();

            _sender = new Sender("telemetry");
        }

        void t_Tick(object sender, EventArgs e)
        {
            Int64 phav = PerformanceInfo.GetPhysicalAvailableMemoryInMiB();
            Int64 tot = PerformanceInfo.GetTotalMemoryInMiB();
            decimal percentFree = ((decimal)phav / (decimal)tot) * 100;
            decimal percentOccupied = 100 - percentFree;

            float cpu = cpuCounter.NextValue(), ram = (float)percentOccupied;
            cpuLabel.Text = cpu.ToString();
            memoryLabel.Text = ram.ToString();
            _sender.SendProcessorAsync(cpu, ram);
        }

        void listener_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            mouseXLabel.Text = e.X.ToString();
            mouseYLabel.Text = e.Y.ToString();
            _sender.SendMouseAsync(e.X, e.Y);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _mouseListener.Start();
            startMouseTrackingButton.Enabled = false;
            stopMouseTrackingButton.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            startMouseTrackingButton.Enabled = true;
            stopMouseTrackingButton.Enabled = false;

            _mouseListener.Stop();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var pos=System.Windows.Forms.Cursor.Position;
            _sender.SendMouseAsync(pos.X, pos.Y);
        }

        private void startCPUTrackingButton_Click(object sender, EventArgs e)
        {
            t.Start();
            startCPUTrackingButton.Enabled = false;
            stopCpuTrackingButton.Enabled = true;
        }

        private void stopCpuTrackingButton_Click(object sender, EventArgs e)
        {
            if (t.Enabled)
                t.Stop();
            startCPUTrackingButton.Enabled = true;
            stopCpuTrackingButton.Enabled = false;
        }
    }
}
