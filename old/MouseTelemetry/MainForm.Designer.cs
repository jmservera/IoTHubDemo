namespace MouseTelemetry
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.mouseXLabel = new System.Windows.Forms.Label();
            this.mouseYLabel = new System.Windows.Forms.Label();
            this.startMouseTrackingButton = new System.Windows.Forms.Button();
            this.stopMouseTrackingButton = new System.Windows.Forms.Button();
            this.t = new System.Windows.Forms.Timer(this.components);
            this.cpuLabel = new System.Windows.Forms.Label();
            this.memoryLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.startCPUTrackingButton = new System.Windows.Forms.Button();
            this.stopCpuTrackingButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // mouseXLabel
            // 
            this.mouseXLabel.AutoSize = true;
            this.mouseXLabel.Location = new System.Drawing.Point(233, 9);
            this.mouseXLabel.Name = "mouseXLabel";
            this.mouseXLabel.Size = new System.Drawing.Size(31, 32);
            this.mouseXLabel.TabIndex = 0;
            this.mouseXLabel.Text = "0";
            // 
            // mouseYLabel
            // 
            this.mouseYLabel.AutoSize = true;
            this.mouseYLabel.Location = new System.Drawing.Point(233, 51);
            this.mouseYLabel.Name = "mouseYLabel";
            this.mouseYLabel.Size = new System.Drawing.Size(31, 32);
            this.mouseYLabel.TabIndex = 0;
            this.mouseYLabel.Text = "0";
            // 
            // startMouseTrackingButton
            // 
            this.startMouseTrackingButton.Location = new System.Drawing.Point(19, 116);
            this.startMouseTrackingButton.Name = "startMouseTrackingButton";
            this.startMouseTrackingButton.Size = new System.Drawing.Size(165, 68);
            this.startMouseTrackingButton.TabIndex = 1;
            this.startMouseTrackingButton.Text = "Start";
            this.startMouseTrackingButton.UseVisualStyleBackColor = true;
            this.startMouseTrackingButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // stopMouseTrackingButton
            // 
            this.stopMouseTrackingButton.Location = new System.Drawing.Point(202, 116);
            this.stopMouseTrackingButton.Name = "stopMouseTrackingButton";
            this.stopMouseTrackingButton.Size = new System.Drawing.Size(165, 68);
            this.stopMouseTrackingButton.TabIndex = 1;
            this.stopMouseTrackingButton.Text = "Stop";
            this.stopMouseTrackingButton.UseVisualStyleBackColor = true;
            this.stopMouseTrackingButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // t
            // 
            this.t.Interval = 1000;
            this.t.Tick += new System.EventHandler(this.t_Tick);
            // 
            // cpuLabel
            // 
            this.cpuLabel.AutoSize = true;
            this.cpuLabel.Location = new System.Drawing.Point(233, 291);
            this.cpuLabel.Name = "cpuLabel";
            this.cpuLabel.Size = new System.Drawing.Size(31, 32);
            this.cpuLabel.TabIndex = 0;
            this.cpuLabel.Text = "0";
            // 
            // memoryLabel
            // 
            this.memoryLabel.AutoSize = true;
            this.memoryLabel.Location = new System.Drawing.Point(233, 333);
            this.memoryLabel.Name = "memoryLabel";
            this.memoryLabel.Size = new System.Drawing.Size(31, 32);
            this.memoryLabel.TabIndex = 0;
            this.memoryLabel.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 32);
            this.label1.TabIndex = 2;
            this.label1.Text = "Mouse X:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(134, 32);
            this.label2.TabIndex = 2;
            this.label2.Text = "Mouse Y:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 291);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 32);
            this.label5.TabIndex = 2;
            this.label5.Text = "CPU: ";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 332);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(124, 32);
            this.label6.TabIndex = 2;
            this.label6.Text = "Memory:";
            // 
            // startCPUTrackingButton
            // 
            this.startCPUTrackingButton.Location = new System.Drawing.Point(19, 403);
            this.startCPUTrackingButton.Name = "startCPUTrackingButton";
            this.startCPUTrackingButton.Size = new System.Drawing.Size(165, 68);
            this.startCPUTrackingButton.TabIndex = 1;
            this.startCPUTrackingButton.Text = "Start";
            this.startCPUTrackingButton.UseVisualStyleBackColor = true;
            this.startCPUTrackingButton.Click += new System.EventHandler(this.startCPUTrackingButton_Click);
            // 
            // stopCpuTrackingButton
            // 
            this.stopCpuTrackingButton.Location = new System.Drawing.Point(202, 403);
            this.stopCpuTrackingButton.Name = "stopCpuTrackingButton";
            this.stopCpuTrackingButton.Size = new System.Drawing.Size(165, 68);
            this.stopCpuTrackingButton.TabIndex = 1;
            this.stopCpuTrackingButton.Text = "Stop";
            this.stopCpuTrackingButton.UseVisualStyleBackColor = true;
            this.stopCpuTrackingButton.Click += new System.EventHandler(this.stopCpuTrackingButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1230, 707);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.stopCpuTrackingButton);
            this.Controls.Add(this.stopMouseTrackingButton);
            this.Controls.Add(this.startCPUTrackingButton);
            this.Controls.Add(this.startMouseTrackingButton);
            this.Controls.Add(this.memoryLabel);
            this.Controls.Add(this.cpuLabel);
            this.Controls.Add(this.mouseYLabel);
            this.Controls.Add(this.mouseXLabel);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label mouseXLabel;
        private System.Windows.Forms.Label mouseYLabel;
        private System.Windows.Forms.Button startMouseTrackingButton;
        private System.Windows.Forms.Button stopMouseTrackingButton;
        private System.Windows.Forms.Timer t;
        private System.Windows.Forms.Label cpuLabel;
        private System.Windows.Forms.Label memoryLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button startCPUTrackingButton;
        private System.Windows.Forms.Button stopCpuTrackingButton;
    }
}