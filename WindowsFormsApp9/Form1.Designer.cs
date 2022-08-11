namespace WinFormsApp2
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private OxyPlot.WindowsForms.PlotView plotView_steer;
        private System.Windows.Forms.TabPage tabPage2;
        private OxyPlot.WindowsForms.PlotView plotView_throttle;
        private System.Windows.Forms.TabPage tabPage3;
        private OxyPlot.WindowsForms.PlotView plotView_brake;
        private System.Windows.Forms.TabPage tabPage4;
        private OxyPlot.WindowsForms.PlotView plotView_clutch;
        private System.Windows.Forms.TabPage tabPage5;
        private OxyPlot.WindowsForms.PlotView plotView_handbrake;
        private System.Windows.Forms.TabPage tabPage6;
        private OxyPlot.WindowsForms.PlotView plotView_gear;
        private System.Windows.Forms.Button LfsTelemetryButton;
        private System.Windows.Forms.Button GetResultButton;
        private System.Windows.Forms.Button RbrTelemetryButton;
        private System.Windows.Forms.Button DirtRallyButton;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TabPage tabPage7;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_axis_gearup;
        private System.Windows.Forms.ComboBox cb_device_gearup;
        private System.Windows.Forms.ComboBox comboBox_axis_geardown;
        private System.Windows.Forms.ComboBox cb_device_geardown;
        private System.Windows.Forms.TrackBar trackBar_Handbrake;
        private System.Windows.Forms.CheckBox checkBox_handbrake;
        private System.Windows.Forms.ComboBox comboBox_axis_handbrake;
        private System.Windows.Forms.ComboBox cb_device_handbrake;
        private System.Windows.Forms.TrackBar trackBar_clutch;
        private System.Windows.Forms.CheckBox checkBox_clutch;
        private System.Windows.Forms.ComboBox comboBox_axis_clutch;
        private System.Windows.Forms.ComboBox cb_device_clutch;
        private System.Windows.Forms.TrackBar trackBar_brake;
        private System.Windows.Forms.CheckBox checkBox_brake;
        private System.Windows.Forms.ComboBox comboBox_axis_brake;
        private System.Windows.Forms.ComboBox cb_device_brake;
        private System.Windows.Forms.TrackBar trackBar_throttle;
        private System.Windows.Forms.CheckBox checkBox_throttle;
        private System.Windows.Forms.ComboBox comboBox_axis_throttle;
        private System.Windows.Forms.ComboBox cb_device_throttle;
        private System.Windows.Forms.TrackBar trackBar_steering;
        private System.Windows.Forms.CheckBox checkBox_steer;
        private System.Windows.Forms.ComboBox comboBox_axis_steering;
        private System.Windows.Forms.ComboBox cb_device_steer;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button RequestDeviceData;
        private System.Windows.Forms.Button GetDevices;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.CheckBox checkBox_clutch_is_axis;
        private System.Windows.Forms.CheckBox checkBox_handbrake_is_axis;
        private System.Windows.Forms.Button button_savesettings;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button acc_telemetry_button;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button DeviceLatencyTester;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.MaskedTextBox export_mp;
        private System.Windows.Forms.MaskedTextBox ac_delay;
        private System.Windows.Forms.MaskedTextBox fh_udp_tb;
        private System.Windows.Forms.MaskedTextBox dr_udp_tb;
        private System.Windows.Forms.MaskedTextBox rbr_udp_tb;
        private System.Windows.Forms.MaskedTextBox lfs_udp_tb;
        private System.Windows.Forms.TabPage tabPage8;
        private OxyPlot.WindowsForms.PlotView plotView1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label speed_txt;
    }
}
