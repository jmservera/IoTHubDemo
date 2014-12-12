using Microsoft.ServiceBus.Messaging;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

        public MainForm()
        {
            //GlobalProxySelection.Select = new WebProxy("127.0.0.1", 8888);

            InitializeComponent();
            _mouseListener = new MouseHookListener(new GlobalHooker());
            _mouseListener.MouseMove += listener_MouseMove;

            _sender = new Sender("telemetry");
        }

        void listener_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            label1.Text = e.X.ToString();
            label2.Text = e.Y.ToString();
            _sender.SendAsync(e.X, e.Y);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _mouseListener.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _mouseListener.Stop();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var pos=System.Windows.Forms.Cursor.Position;
            _sender.SendAsync(pos.X, pos.Y);
        }
    }
}
