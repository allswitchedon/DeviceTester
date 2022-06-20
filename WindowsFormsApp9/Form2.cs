using OxyPlot;
using OxyPlot.Series;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp2
{
    public partial class Form2 : Form
    {

        DirectInput di;
        Joystick js;
        Thread tr;
        Thread tr2;


        List<JoystickState> st;
        Stopwatch sw;
        List<long> time;
        IList<DeviceInstance> dd;
        List<string> dn = new List<string>();
        List<string> dx = new List<string>();
        PlotModel pm = new PlotModel();
        LineSeries ls = new LineSeries();
        int d_id;
        string d_x;

        public Form2()
        {
            InitializeComponent();
        }

        public void JS_Get_State()
        {
            
            while (sw.ElapsedMilliseconds < 3000)
            {
                st.Add(js.GetCurrentState());
                time.Add(DateTime.Now.Ticks);               
            }
            sw.Stop();
            js.Unacquire();
                CalcHz();

            
        }

        void pb_upd()
        {
            while (sw.ElapsedMilliseconds < 3000)
            {
                progressBar1.Invoke((MethodInvoker)(() => progressBar1.Value = Convert.ToInt16(sw.ElapsedMilliseconds / 30)));
                Thread.Sleep(100);
            }
            progressBar1.Invoke((MethodInvoker)(() => progressBar1.Value = 100));
        }

        void CalcHz()
        {
            PlotModel pm = new PlotModel();
            LineSeries ls = new LineSeries();
            var time_min = time.Min();
            float vl_cr = Convert.ToSingle(st[0].GetType().GetProperty(d_x).GetValue(st[d_id]));
            float vl_pr = Convert.ToSingle(st[0].GetType().GetProperty(d_x).GetValue(st[d_id]));
            float time_df = 0;
            List<float> hz = new List<float>();
            List<float> ms = new List<float>();
            for (int i = 0; i < st.Count; i++)
            {
                ms.Add((float)(time[i] - time_min) / 10000000);
                ls.Points.Add(new DataPoint((float)(time[i] - time_min) / 10000000, Convert.ToSingle(st[i].GetType().GetProperty(d_x).GetValue(st[i]))));
            }
            float time_pr = ms[0];
            float time_cr = ms[0];
            for (int i = 0; i < st.Count; i++)
            {
                time_cr = ms[i];
                vl_cr = Convert.ToSingle(st[i].GetType().GetProperty(d_x).GetValue(st[i]));
                if (vl_cr != vl_pr)
                {
                    time_df = time_cr - time_pr;
                    if (time_df > 0)
                        hz.Add(time_df);
                    time_pr = time_cr;
                    vl_pr = vl_cr;
                }
                else; 
                    //time_pr = time_cr;
            }
            pm.Series.Add(ls);
            plotView1.Model = pm;
            string st_result = "";
            int cases=1;
            if (rb_10.Checked == true)
                cases = 10;
            if (rb_20.Checked == true)
                 cases = 20;
            if (rb_50.Checked == true)
                 cases = 50;
            if (rb_100.Checked == true)
                 cases = 100;
            float[] ft_test = new float[cases];
            try
            {
                
                for (int i = 0; i < cases; i++)
                {
                    ft_test[i] = hz.Min();
                    var hz_min = hz.Min();
                    hz.RemoveAll(hz_min.Equals);
                }
            
            float hz_result = 0;
            
            
                hz_result = 1 / ft_test.Average();
                st_result = hz_result.ToString();
            }
            catch { 
                st_result = "N/A"; 
            }
            label4.Invoke((MethodInvoker)(() => label4.Text = st_result + " Hz"));
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                d_x = comboBox2.Text;
                d_id = comboBox1.SelectedIndex;
                js = new Joystick(di, dd[d_id].InstanceGuid);
                js.Acquire();
                var max = js.GetObjectPropertiesByName(d_x).Range.Maximum;
                time = new List<long>();
                st = new List<JoystickState>();
                tr = new Thread(JS_Get_State);
                tr2 = new Thread(pb_upd);
                sw = new Stopwatch();


                //sw.Start();
                sw.Start();
                tr.Start();
                tr2.Start();
            }
            catch
            {
                MessageBox.Show("Worng Axis");
            }
        }


        private void Form2_Load(object sender, EventArgs e)
        {
            di = new DirectInput();
            dd = di.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AllDevices);
            for (int i = 0; i < dd.Count; i++)
                dn.Add(dd[i].InstanceName);
            //dx.AddRange(new List<string> { "X", "Y", "Z", "RotationX", "RotationY", "RotationZ", "Sliders 0", "Sliders 1" });
            dx.AddRange(new List<string> { "X", "Y", "Z", "RotationX", "RotationY", "RotationZ"});
            comboBox1.Items.AddRange(dn.ToArray());
            comboBox2.Items.AddRange(dx.ToArray());
            rb_10.Checked = true;
            
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (st != null & time != null)
            {
                st.Clear();
                time.Clear();
            }
            GC.Collect();
        }
    }
}
