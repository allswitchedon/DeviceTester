using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Legends;
using OxyPlot.Series;
using SharpDX.DirectInput;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using OxyPlot.WindowsForms;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Numerics;
using System.Windows;

namespace WinFormsApp2
{
    public partial class Form1 : Form
    {
        IList<DeviceInstance> deviceslist;
        DirectInput directinput = new DirectInput();
        DataTable devicestable = new DataTable();
        List<object> devices_name = new List<object>();
        List<Guid> devices_id = new List<Guid>();
        List<string> devices_id_str = new List<string>();
        List<object> device_axis_list = new List<object>();
        List<object> device_button_list = new List<object>();
        List<Joystick> joysticks_list = new List<Joystick>();
        //List<JoystickState> joystickStates_list = new List<JoystickState>();
        JoystickState[] test = new JoystickState[0];
        List<JoystickState[]> joystickStates_list_all = new List<JoystickState[]>();

        Stopwatch sw;

        bool isconnected = false;
        bool Settings_enable = false;
        bool acc_telemetry = false;

        UdpClient udpclient = new UdpClient();
        IPEndPoint remoteIP = new IPEndPoint(IPAddress.Any, 0);
        List<long> DateTimeTicks = new List<long>();
        byte[] income_upd;

        LiveForSpeedTelemetryAPI lfs_api;
        RichardBurnsRallyNGPTelemetryAPI rbr_api;
        DirtRallyTelemetryAPI dirt_api;
        ForzaHorizonAPI forza_api;
        ACCsPageFilePhysics acc_api;

        Thread tr;

        MemoryMappedFile sharedMemory;

        List<List<float>> inputlist = new List<List<float>>();
        List<List<float>> outputlist = new List<List<float>>();

        string settingspath = Application.StartupPath.ToString() + "\\devices\\";


        int steer_id_dv = 0;
        int throttle_id_dv = 0;
        int brake_id_dv = 0;
        int clutch_id_dv = 0;
        int handbrake_id_dv=0;
        int gearup_id_dv = 0;
        int geardown_id_dv = 0;

        int acc_delay = 16;

        string steer_axis = "";
        string throttle_axis = "";
        string brake_axis = "";
        string clutch_axis = "";
        string handbrake_axis = "";
        string gearup_axis = "";
        string geardown_axis = "";

        float steer_max = 0;
        float steer_half = 0;
        float throttle_max = 0;
        float brake_max = 0;
        float clutch_max = 0;
        float handbrake_max = 0;
        float mp = 1;

        int lfs_udp_port = 0;
        int rbr_udp_port = 0;
        int dr_udp_port = 0;
        int fh_udp_port = 0;

        bool steer_inverted , throttle_inverted, brake_inverted, clutch_inverted, handbrake_inverted, handbrake_isaxis, clutch_isaxis;


        int totalstate = 0;

        string str_dv, thr_dv, brk_dv, clt_dv, hbk_dv, gup_dv, gdw_dv;


        List<object> fafs = new List<object>();

        float[] a_b_p = new float[]{ 0,0,0,0,0 };


        public Form1()
        {
            InitializeComponent();            
            
            devicestable.Columns.Add("InstanceName");
            devicestable.Columns.Add("Type");
            devicestable.Columns.Add("Axis");
            devicestable.Columns.Add("Buttons");
            devicestable.Columns.Add("X");
            devicestable.Columns.Add("Y");
            devicestable.Columns.Add("Z");
            devicestable.Columns.Add("RX");
            devicestable.Columns.Add("RY");
            devicestable.Columns.Add("RZ");
            devicestable.Columns.Add("S0");
            devicestable.Columns.Add("S1");
        }

        // CLEAN SECTION

        async void ClearData()
        {

            byte[] income_upd;
            inputlist.Clear();
            outputlist.Clear();
            DateTimeTicks.Clear();
            totalstate = 0;
            joystickStates_list_all.Clear();
            Settings_enable = false ;
            
        }

        // Live For Speed Section

        private async void LFSTelemetryButtonClick(object sender, EventArgs e)
        {
            acc_telemetry = false;
            if (udpclient.Client.IsBound == true || isconnected == true)
            {
                GetResultButtonClick(sender, null);
                await Task.Delay(100);
            }
            ClearData();
            try
            {
                CheckSettings();
            }
            catch
            {
                if (steer_id_dv == -1 || throttle_id_dv == -1 || brake_id_dv == -1 || clutch_id_dv == -1 || handbrake_id_dv == -1 || gearup_id_dv == -1 || geardown_id_dv == -1)
                {
                    MessageBox.Show(String.Format("Device Not Found"));
                }
                else
                {
                    MessageBox.Show(String.Format("Wrong Setting"));
                }
                tabControl1.SelectTab(tabPage7);
                goto End;
            }

            JoystickState[] test = new JoystickState[joysticks_list.Count];
            
            foreach (Joystick js in joysticks_list)
                js.Acquire();
            remoteIP = new IPEndPoint(IPAddress.Loopback, lfs_udp_port);
            udpclient.Client.Bind(remoteIP);
            isconnected = udpclient.Client.IsBound;
            try
            {
                udpclient.BeginReceive(new AsyncCallback(LFSrecv), null);
                statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = "Waiting LFS Telemetry"));
            }
            catch (Exception ex)
            {
                statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = ex.Message.ToString()));
            }
        End:;
        }

        public void LFSrecv(IAsyncResult LFSres)
        {
            if (isconnected == false)
                goto Endofrecv;
            else
            {
                JoystickState[] js = new JoystickState[joysticks_list.Count];
                long ticks = 0;
                Task[] test = new Task[3];
                test[0] = Task.Run(() => ticks =DateTime.Now.Ticks);
                test[1] = Task.Run(() => income_upd = udpclient.EndReceive(LFSres, ref remoteIP));
                test[2] = Task.Run(() =>
                {                    
                    for (int i = 0; i < js.Length; i++)
                    {
                        js[i] = joysticks_list[i].GetCurrentState();
                    }
                }
                );
                Task.WaitAll(test);
                if (income_upd.Length == 280)
                {
                    GCHandle gcHandle = GCHandle.Alloc((object)income_upd, GCHandleType.Pinned);
                    lfs_api = (LiveForSpeedTelemetryAPI)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(LiveForSpeedTelemetryAPI));
                    gcHandle.Free();
                    outputlist.Add(new List<float> { (float)(lfs_api.OSInputs.InputSteer / 0.6282994151115417 * -1), lfs_api.OSInputs.Throttle, lfs_api.OSInputs.Brake, lfs_api.OSInputs.Clutch, lfs_api.OSInputs.Handbrake, lfs_api.Gear });
                    joystickStates_list_all.Add(js);
                    DateTimeTicks.Add(ticks);

                    totalstate++;
                    statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = "Reciving LFS Telemetry: " + totalstate.ToString()));
                }
                if (isconnected == true)
                    udpclient.BeginReceive(new AsyncCallback(LFSrecv), null);
                
            }
        Endofrecv:;
        }
        //end


        //public void LFSrecv(IAsyncResult LFSres)
        //{ 
        //    if (isconnected == false)
        //        goto Endofrecv;
        //    else
        //    {
        //        income_upd = udpclient.EndReceive(LFSres, ref remoteIP);
        //        if (income_upd.Length == 280)
        //        {
        //            GCHandle gcHandle = GCHandle.Alloc((object)income_upd, GCHandleType.Pinned);
        //            lfs_api = (LiveForSpeedTelemetryAPI)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(LiveForSpeedTelemetryAPI));
        //            gcHandle.Free();
        //            //inputlist.Add(GameController.TrustmasterT300());
        //            Task[] test = new Task[3];
        //            test[0] = Task.Run(() => DateTimeTicks.Add(DateTime.Now.Ticks));
        //            test[1] = Task.Run(() => outputlist.Add(new List<float> { (float)(lfs_api.OSInputs.InputSteer / 0.6282994151115417 * -1), lfs_api.OSInputs.Throttle, lfs_api.OSInputs.Brake, lfs_api.OSInputs.Clutch, lfs_api.OSInputs.Handbrake, lfs_api.Gear }));
        //            test[2] = Task.Run(() =>
        //            {
        //                JoystickState[] js = new JoystickState[joysticks_list.Count];
        //                for (int i = 0; i < js.Length; i++)
        //                {
        //                    js[i] = joysticks_list[i].GetCurrentState();
        //                }
        //                joystickStates_list_all.Add(js);
        //            }
        //            );
        //            Task.WaitAll(test);
        //            totalstate++;
        //            statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = "Reciving LFS Telemetry: " + totalstate.ToString()));
        //        }
        //        if (isconnected == true)
        //            udpclient.BeginReceive(new AsyncCallback(LFSrecv), null);
        //    }
        //Endofrecv:;
        //}
        ////end

        //Richard Burns Rally NGP 6 Section

        private async void RBR_Telemetry_Button_Click(object sender, EventArgs e)
        {
            acc_telemetry = false;
            if (udpclient.Client.IsBound == true)
            {
                GetResultButtonClick(sender, null);
                await Task.Delay(100);
            }
            ClearData();
            try
            {
                CheckSettings();
            }
            catch
            {
                if (steer_id_dv == -1 || throttle_id_dv == -1 || brake_id_dv == -1 || clutch_id_dv == -1 || handbrake_id_dv == -1 || gearup_id_dv == -1 || geardown_id_dv == -1)
                {
                    MessageBox.Show(String.Format("Device Not Found"));
                    label4.Invoke((MethodInvoker)(() => label4.Text = "Wrong Setting"));
                }
                else
                {
                    MessageBox.Show(String.Format("Wrong Setting"));
                    label4.Invoke((MethodInvoker)(() => label4.Text = "Wrong Setting"));
                }

                tabControl1.SelectTab(tabPage7);
                goto End;
            }
            JoystickState[] test = new JoystickState[joysticks_list.Count];

            foreach (Joystick js in joysticks_list)
                js.Acquire();
            remoteIP = new IPEndPoint(IPAddress.Loopback, rbr_udp_port);
            udpclient.Client.Bind(remoteIP);
            isconnected = udpclient.Client.IsBound;
            try
            {
                udpclient.BeginReceive(new AsyncCallback(RBRrecv), null);
                statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = "Waiting RBR Telemetry"));
            }
            catch (Exception ex)
            {
                statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = ex.Message.ToString()));
            }
        End:;
        }

        public void RBRrecv(IAsyncResult RBRres)
        {
            if (isconnected == false)
                goto Endofrecv;
            else
            {
                JoystickState[] js = new JoystickState[joysticks_list.Count];
                long ticks = 0;
                Task[] test = new Task[3];
                test[0] = Task.Run(() => ticks = DateTime.Now.Ticks);
                test[1] = Task.Run(() => income_upd = udpclient.EndReceive(RBRres, ref remoteIP));
                test[2] = Task.Run(() =>
                {
                    for (int i = 0; i < js.Length; i++)
                    {
                        js[i] = joysticks_list[i].GetCurrentState();
                    }
                }
                );
                Task.WaitAll(test);
                if (income_upd.Length == 664)
                {
                    GCHandle gcHandle = GCHandle.Alloc((object)income_upd, GCHandleType.Pinned);
                    rbr_api = (RichardBurnsRallyNGPTelemetryAPI)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(RichardBurnsRallyNGPTelemetryAPI));
                    gcHandle.Free();
                    outputlist.Add(new List<float> { rbr_api.control.steering, rbr_api.control.throttle, rbr_api.control.brake, rbr_api.control.clutch, rbr_api.control.handbrake, rbr_api.control.gear }); 
                    joystickStates_list_all.Add(js);
                    DateTimeTicks.Add(ticks);
                    totalstate++;
                    statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = "Reciving RBR Telemetry: " + totalstate.ToString()));
                }
                if (isconnected == true)
                    udpclient.BeginReceive(new AsyncCallback(RBRrecv), null);
            }
        Endofrecv:;
        }
        //end


        //public void RBRrecv(IAsyncResult RBRres)
        //{
        //    if (isconnected == false)
        //        goto Endofrecv;
        //    else
        //    {
        //        income_upd = udpclient.EndReceive(RBRres, ref remoteIP);
        //        if (income_upd.Length == 664)
        //        {
        //            GCHandle gcHandle = GCHandle.Alloc((object)income_upd, GCHandleType.Pinned);
        //            rbr_api = (RichardBurnsRallyNGPTelemetryAPI)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(RichardBurnsRallyNGPTelemetryAPI));
        //            gcHandle.Free();
        //            Task[] test = new Task[3];
        //            test[0] = Task.Run(() => DateTimeTicks.Add(DateTime.Now.Ticks));
        //            test[1] = Task.Run(() => outputlist.Add(new List<float> { rbr_api.control.steering, rbr_api.control.throttle, rbr_api.control.brake, rbr_api.control.clutch, rbr_api.control.handbrake, rbr_api.control.gear }));
        //            test[2] = Task.Run(() =>
        //            {
        //                JoystickState[] js = new JoystickState[joysticks_list.Count];
        //                for (int i = 0; i < js.Length; i++)
        //                {
        //                    js[i] = joysticks_list[i].GetCurrentState();
        //                }
        //                joystickStates_list_all.Add(js);
        //            }
        //            );
        //            Task.WaitAll(test);
        //            totalstate++;
        //            statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = "Reciving RBR Telemetry: " + totalstate.ToString()));
        //        }
        //        if (isconnected == true)
        //            udpclient.BeginReceive(new AsyncCallback(RBRrecv), null);
        //    }
        //Endofrecv:;
        //}





        // rbr end


        // Dirt Rally Section

        private async void Dirt_Rally_Button_Click(object sender, EventArgs e)
        {
            acc_telemetry = false;
            if (udpclient.Client.IsBound == true)
            {
                GetResultButtonClick(sender, null);
                await Task.Delay(100);
            }
            ClearData();
            try
            {
                CheckSettings();
            }
            catch
            {
                if (steer_id_dv == -1 || throttle_id_dv == -1 || brake_id_dv == -1 || clutch_id_dv == -1 || handbrake_id_dv == -1 || gearup_id_dv == -1 || geardown_id_dv == -1)
                {
                    MessageBox.Show(String.Format("Device Not Found"));
                    label4.Invoke((MethodInvoker)(() => label4.Text = "Wrong Setting"));
                }
                else
                {
                    MessageBox.Show(String.Format("Wrong Setting"));
                    label4.Invoke((MethodInvoker)(() => label4.Text = "Wrong Setting"));
                }

                tabControl1.SelectTab(tabPage7);
                goto End;
            }
            JoystickState[] test = new JoystickState[joysticks_list.Count];

            foreach (Joystick js in joysticks_list)
                js.Acquire();
            remoteIP = new IPEndPoint(IPAddress.Loopback, dr_udp_port);
            udpclient.Client.Bind(remoteIP);
            isconnected = udpclient.Client.IsBound;
            try
            {
                udpclient.BeginReceive(new AsyncCallback(Dirtrecv), null);
                statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = "Waiting Dirt Rally Telemetry"));
            }
            catch (Exception ex)
            {
                statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = ex.Message.ToString()));
            }
        End:;
        }


        public void Dirtrecv(IAsyncResult Dirtres)
        {
            if (isconnected == false)
                goto Endofrecv;
            else
            {
                JoystickState[] js = new JoystickState[joysticks_list.Count];
                long ticks = 0;
                Task[] test = new Task[3];
                test[0] = Task.Run(() => ticks = DateTime.Now.Ticks);
                test[1] = Task.Run(() => income_upd = udpclient.EndReceive(Dirtres, ref remoteIP));
                test[2] = Task.Run(() =>
                {
                    for (int i = 0; i < js.Length; i++)
                    {
                        js[i] = joysticks_list[i].GetCurrentState();
                    }
                }
                );
                Task.WaitAll(test);
                if (income_upd.Length == 264)
                {
                    GCHandle gcHandle = GCHandle.Alloc((object)income_upd, GCHandleType.Pinned);
                    dirt_api = (DirtRallyTelemetryAPI)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(DirtRallyTelemetryAPI));
                    gcHandle.Free();
                    outputlist.Add(new List<float> { dirt_api.Steer, dirt_api.Throttle, dirt_api.Brake, dirt_api.Clutch, 0, dirt_api.Gear });
                    joystickStates_list_all.Add(js);
                    DateTimeTicks.Add(ticks);
                    totalstate++;
                    statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = "Reciving Dirt Rally Telemetry: " + totalstate.ToString()));
                }
                if (isconnected == true)
                    udpclient.BeginReceive(new AsyncCallback(Dirtrecv), null);
            }
        Endofrecv:;
        }
        //end



        //public void Dirtrecv(IAsyncResult Dirtres)
        //{
        //    if (isconnected == false)
        //        goto Endofrecv;
        //    else
        //    {
        //        income_upd = udpclient.EndReceive(Dirtres, ref remoteIP);
        //        if (income_upd.Length == 264)
        //        {
        //            GCHandle gcHandle = GCHandle.Alloc((object)income_upd, GCHandleType.Pinned);
        //            dirt_api = (DirtRallyTelemetryAPI)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(DirtRallyTelemetryAPI));
        //            gcHandle.Free();
        //            Task[] test = new Task[3];
        //            test[0] = Task.Run(() => DateTimeTicks.Add(DateTime.Now.Ticks));
        //            test[1] = Task.Run(() => outputlist.Add(new List<float> { dirt_api.Steer, dirt_api.Throttle, dirt_api.Brake, dirt_api.Clutch, 0, dirt_api.Gear }));
        //            test[2] = Task.Run(() =>
        //            {
        //                JoystickState[] js = new JoystickState[joysticks_list.Count];
        //                for (int i = 0; i < js.Length; i++)
        //                {
        //                    js[i] = joysticks_list[i].GetCurrentState();
        //                }
        //                joystickStates_list_all.Add(js);
        //            }
        //            );
        //            Task.WaitAll(test);
        //            totalstate++;
        //            statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = "Reciving Dirt Rally Telemetry: " + totalstate.ToString()));
        //        }
        //        if (isconnected == true)
        //            udpclient.BeginReceive(new AsyncCallback(Dirtrecv), null);
        //    }
        //Endofrecv:;
        //}
        // dirt end


        
        // Forza Horizon Section

        private async void Forza_Horizon_Button_Click(object sender, EventArgs e)
        {
            acc_telemetry = false;
            if (udpclient.Client.IsBound == true)
            {
                GetResultButtonClick(sender, null);
                await Task.Delay(100);
            }
            ClearData();
            try
            {
                CheckSettings();
            }
            catch
            {
                if (steer_id_dv == -1 || throttle_id_dv == -1 || brake_id_dv == -1 || clutch_id_dv == -1 || handbrake_id_dv == -1 || gearup_id_dv == -1 || geardown_id_dv == -1)
                {
                    MessageBox.Show(String.Format("Device Not Found"));
                    label4.Invoke((MethodInvoker)(() => label4.Text = "Wrong Setting"));
                }
                else
                {
                    MessageBox.Show(String.Format("Wrong Setting"));
                    label4.Invoke((MethodInvoker)(() => label4.Text = "Wrong Setting"));
                }

                tabControl1.SelectTab(tabPage7);
                goto End;
            }
            JoystickState[] test = new JoystickState[joysticks_list.Count];

            foreach (Joystick js in joysticks_list)
                js.Acquire();
            remoteIP = new IPEndPoint(IPAddress.Loopback, fh_udp_port);
            udpclient.Client.Bind(remoteIP);
            isconnected = udpclient.Client.IsBound;
            try
            {
                udpclient.BeginReceive(new AsyncCallback(Forzarecv), null);
                statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = "Waiting Forza Horizon Telemetry"));
            }
            catch (Exception ex)
            {
                statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = ex.Message.ToString()));
            }
        End:;
        }

        public void Forzarecv(IAsyncResult Forzares)
        {
            if (isconnected == false)
                goto Endofrecv;
            else
            {
                JoystickState[] js = new JoystickState[joysticks_list.Count];
                long ticks = 0;
                Task[] test = new Task[3];
                test[0] = Task.Run(() => ticks = DateTime.Now.Ticks);
                test[1] = Task.Run(() => income_upd = udpclient.EndReceive(Forzares, ref remoteIP));
                test[2] = Task.Run(() =>
                {
                    for (int i = 0; i < js.Length; i++)
                    {
                        js[i] = joysticks_list[i].GetCurrentState();
                    }
                }
                );
                Task.WaitAll(test);
                if (income_upd.Length == 324)
                {
                    GCHandle gcHandle = GCHandle.Alloc((object)income_upd, GCHandleType.Pinned);
                    forza_api = (ForzaHorizonAPI)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(ForzaHorizonAPI));
                    gcHandle.Free();
                    float steer_value;;
                    if (forza_api.Steer < 128)
                        steer_value = (float)forza_api.Steer / 127;
                    else
                        steer_value = ((float)forza_api.Steer - 256) / 127;
                    outputlist.Add(new List<float> { steer_value, (float)forza_api.Throttle / 255, (float)forza_api.Brake / 255, (float)forza_api.Clutch / 255, (float)forza_api.Handbrake / 255, (float)forza_api.Gear / 255 });
                    joystickStates_list_all.Add(js);
                    DateTimeTicks.Add(ticks);
                    totalstate++;
                    statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = "Reciving Forza Horizon Telemetry: " + totalstate.ToString()));
                }
                if (isconnected == true)
                    udpclient.BeginReceive(new AsyncCallback(Forzarecv), null);
            }
        Endofrecv:;
        }
        //end





        //public void Forzarecv(IAsyncResult Forzares)
        //{
        //    if (isconnected == false)
        //        goto Endofrecv;
        //    else
        //    {
        //        income_upd = udpclient.EndReceive(Forzares, ref remoteIP);
        //        if (income_upd.Length == 324)
        //        {
        //            GCHandle gcHandle = GCHandle.Alloc((object)income_upd, GCHandleType.Pinned);
        //            forza_api = (ForzaHorizonAPI)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(ForzaHorizonAPI));
        //            gcHandle.Free();
        //            Task[] test = new Task[3];
        //            test[0] = Task.Run(() => DateTimeTicks.Add(DateTime.Now.Ticks));
        //            test[1] = Task.Run(() =>
        //            {
        //                float steer_value;
        //            //if (forza_api.Steer == 0)
        //            //    steer_value = 0;
        //                if (forza_api.Steer < 128)
        //                    steer_value = (float)forza_api.Steer / 127;
        //            //if (forza_api.Steer > 128)
        //                else
        //                    steer_value = ((float)forza_api.Steer - 256) / 127;
        //                outputlist.Add(new List<float> { steer_value, (float)forza_api.Throttle / 255, (float)forza_api.Brake / 255, (float)forza_api.Clutch / 255, (float)forza_api.Handbrake / 255, (float)forza_api.Gear / 255 });
        //            });
        //            test[2] = Task.Run(() =>
        //            {
        //                JoystickState[] js = new JoystickState[joysticks_list.Count];
        //                for (int i = 0; i < js.Length; i++)
        //                {
        //                    js[i] = joysticks_list[i].GetCurrentState();
        //                }
        //                joystickStates_list_all.Add(js);
        //            }
        //            );
        //            Task.WaitAll(test);
        //            totalstate++;
        //            statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = "Reciving Forza Horizon Telemetry: " + totalstate.ToString()));
        //        }
        //        if (isconnected == true)
        //            udpclient.BeginReceive(new AsyncCallback(Forzarecv), null);
        //    }
        //Endofrecv:;
        //}
        // dirt end


        void Get_AssettoCorsa_SharedMemory()
        {
            while (isconnected == true)
            {
                using (MemoryMappedViewAccessor reader = sharedMemory.CreateViewAccessor(0, Marshal.SizeOf(typeof(ACCsPageFilePhysics)), MemoryMappedFileAccess.Read))
                {
                    
                    Task[] test = new Task[3];
                    test[0] = Task.Run(() => DateTimeTicks.Add(DateTime.Now.Ticks));
                    test[1] = Task.Run(() => reader.Read<ACCsPageFilePhysics>(0, out acc_api));
                    test[2] = Task.Run(() =>
                    {
                        JoystickState[] js = new JoystickState[joysticks_list.Count];
                        for (int i = 0; i < js.Length; i++)
                        {
                            js[i] = joysticks_list[i].GetCurrentState();
                        }
                        joystickStates_list_all.Add(js);
                    }
                    );
                    Task.WaitAll(test);
                    outputlist.Add(new List<float> { acc_api.steerAngle, acc_api.gas, acc_api.brake, acc_api.clutch*-1+1, 0, acc_api.gear });
                    totalstate++;
                    statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = "Reciving Assetto Corsa Competizione Telemetry: " + totalstate.ToString()));
                    Thread.Sleep(acc_delay);
                }
                
            }
            sharedMemory.Dispose();
        }

        private async void acc_telemetry_button_Click(object sender, EventArgs e)
        {
            
            if (isconnected == true)
            {
                GetResultButtonClick(sender, null);
                await Task.Delay(100);
            }
            ClearData();
            try
            {
                CheckSettings();
                acc_telemetry = true;
            }
            catch
            {
                if (steer_id_dv == -1 || throttle_id_dv == -1 || brake_id_dv == -1 || clutch_id_dv == -1 || handbrake_id_dv == -1 || gearup_id_dv == -1 || geardown_id_dv == -1)
                {
                    MessageBox.Show(String.Format("Device Not Found"));
                    label4.Invoke((MethodInvoker)(() => label4.Text = "Wrong Setting"));
                }
                else
                {
                    MessageBox.Show(String.Format("Wrong Setting"));
                    label4.Invoke((MethodInvoker)(() => label4.Text = "Wrong Setting"));
                }

                tabControl1.SelectTab(tabPage7);
                goto End;
            }

            JoystickState[] test = new JoystickState[joysticks_list.Count];

            foreach (Joystick js in joysticks_list)
                js.Acquire();

            while (isconnected == false && acc_telemetry == true)
            {
                try
                {
                    sharedMemory = MemoryMappedFile.OpenExisting("Local\\acpmf_physics");
                    isconnected = true;
                }
                catch
                {
                }
                statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = "Waiting Assetto Corsa Competizione Telemetry"));
                await Task.Delay(1);
            }

            if (isconnected == true && acc_telemetry == true)
            {
                tr = new Thread(Get_AssettoCorsa_SharedMemory);
                tr.Start();
            }

            End:;
        }



        //Settings Section


        private void GetDevices_Click(object sender, EventArgs e)
        {
            Settings_enable = false;
            devicestable.Clear();
            deviceslist = directinput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
            statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = deviceslist.Count.ToString() + " Devices Found"));
            for (int i = 0; i < deviceslist.Count; i++)
            {
                devicestable.Rows.Add
                    (
                    deviceslist[i].InstanceName,
                    deviceslist[i].Type,
                    new Joystick(directinput, deviceslist[i].InstanceGuid).Capabilities.AxeCount,
                    new Joystick(directinput, deviceslist[i].InstanceGuid).Capabilities.ButtonCount,
                    isaxisavaliable(i, "X"),
                    isaxisavaliable(i, "Y"),
                    isaxisavaliable(i, "Z"),
                    isaxisavaliable(i, "RotationX"),
                    isaxisavaliable(i, "RotationY"),
                    isaxisavaliable(i, "RotationZ"),
                    isaxisavaliable(i, "Sliders0"),
                    isaxisavaliable(i, "Sliders1")
                    ); ;
            }
            //List<string> list = new List<string>();
            dataGridView1.DataSource = devicestable;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.AutoResizeColumns();

            devices_name.Clear();
            devices_id.Clear();
            devices_id_str.Clear();
            joysticks_list.Clear();
            device_button_list.Clear();

            cb_device_steer.Items.Clear();
            cb_device_throttle.Items.Clear();
            cb_device_brake.Items.Clear();
            cb_device_clutch.Items.Clear();
            cb_device_handbrake.Items.Clear();
            cb_device_gearup.Items.Clear();
            cb_device_geardown.Items.Clear();

            comboBox_axis_steering.Items.Clear();
            comboBox_axis_geardown.Items.Clear();
            comboBox_axis_gearup.Items.Clear();
            comboBox_axis_throttle.Items.Clear();
            comboBox_axis_brake.Items.Clear();
            //comboBox_axis_clutch.Items.Clear();
            //comboBox_axis_handbrake.Items.Clear();

            List<int> button_cap = new List<int>();
            foreach (DeviceInstance di in deviceslist)
            {
                devices_name.Add(di.InstanceName);
                devices_id.Add(di.InstanceGuid);
                devices_id_str.Add(di.InstanceGuid.ToString());
                joysticks_list.Add(new Joystick(directinput, di.InstanceGuid));
                button_cap.Add(new Joystick(directinput, di.InstanceGuid).Capabilities.ButtonCount);
            }
            for (int i = 0; i < button_cap.Max(); i++)
            {
                device_button_list.Add("button: " + i.ToString());
            }
            
            cb_device_steer.Items.AddRange(devices_name.ToArray());
            cb_device_throttle.Items.AddRange(devices_name.ToArray());
            cb_device_brake.Items.AddRange(devices_name.ToArray());
            cb_device_clutch.Items.AddRange(devices_name.ToArray());
            cb_device_handbrake.Items.AddRange(devices_name.ToArray());
            cb_device_gearup.Items.AddRange(devices_name.ToArray());
            cb_device_geardown.Items.AddRange(devices_name.ToArray());

            comboBox_axis_steering.Items.AddRange(device_axis_list.ToArray());
            comboBox_axis_geardown.Items.AddRange(device_button_list.ToArray());
            comboBox_axis_gearup.Items.AddRange(device_button_list.ToArray());
            comboBox_axis_throttle.Items.AddRange(device_axis_list.ToArray());
            comboBox_axis_brake.Items.AddRange(device_axis_list.ToArray());
            checkBox_clutch_is_axis_CheckedChanged(sender, null);
            checkBox_handbrake_is_axis_CheckedChanged(sender, null);

            reading_settings();
        }

        

        private async void RequestDeviceData_Click(object sender, EventArgs e)
        {
            Settings_Section();
        }

        private void ac_delay_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(ac_delay.Text, out acc_delay);
        }

        private async void DeviceLatencyTester_Click(object sender, EventArgs e)
        {
            Settings_enable = true;
            foreach (Joystick js in joysticks_list)
                js.Unacquire();
            new Form2().Show(this);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (plotView_steer.Model != null && plotView_throttle.Model != null && plotView_brake.Model != null && plotView_clutch.Model != null && plotView_handbrake.Model != null && plotView_gear.Model != null)
            {
                string fn = Microsoft.VisualBasic.Interaction.InputBox("Enter folder name:");
                if (fn == "" || fn.Intersect(Path.GetInvalidFileNameChars()).Any() == true)
                {
                    MessageBox.Show("Wrong folder name");
                    button1_Click(sender, null);
                }
                else
                {
                    float.TryParse(export_mp.Text, out mp);
                    if (mp <= 0)
                        mp = 1;
                    PngExporter pngExporter = new PngExporter { Width = (int)(plotView_steer.Width * mp), Height = (int)(plotView_steer.Height * mp) };
                    //PngExporter pngExporter = new PngExporter { Width = 2700, Height = 760};
                    string ps = Application.StartupPath.ToString() + "\\result\\" + fn + "\\";

                    if (Directory.Exists(ps) == false)
                        Directory.CreateDirectory(ps);
                    pngExporter.ExportToFile(plotView_steer.Model, ps + "steer.png");
                    pngExporter.ExportToFile(plotView_throttle.Model, ps + "throttle.png");
                    pngExporter.ExportToFile(plotView_brake.Model, ps + "brake.png");
                    pngExporter.ExportToFile(plotView_clutch.Model, ps + "clutch.png");
                    pngExporter.ExportToFile(plotView_handbrake.Model, ps + "handbrake.png");
                    pngExporter.ExportToFile(plotView_gear.Model, ps + "gear.png");
                }
            }

        }

        private void export_mp_TextChanged(object sender, EventArgs e)
        {
            Regex rx1 = new Regex(@"^\d\.?\d?$");
            if (rx1.IsMatch(export_mp.Text))
            { }
            else export_mp.Text = "1.0";
        }

        private async Task Left_foot_brakingAsync()
        {
            // update devices
            JoystickState[] js_array_test = new JoystickState[joysticks_list.Count];
            float throttle_value_nr = 0;
            float steer_value_nr = 0;
            float brake_value_nr = 0;

            double add_x = 0;
            double add_y = 0;
            double rotation_angle = 0;

            Collection<DataPoint> lfb_line_series_low = new Collection<DataPoint>();
            Collection<DataPoint> lfb_line_series_high = new Collection<DataPoint>();
            Collection<DataPoint> brake_pedal = new Collection<DataPoint>();
            Collection<DataPoint> Steering_wheel = new Collection<DataPoint>();
            Collection<DataPoint> Braking_Line = new Collection<DataPoint>();

            double dtt = 0;
            double dttp = 0;
            float veh_speed = 200 / 36 * 10;
            for (int i = 0; i < joysticks_list.Count; i++)
            {
                js_array_test[i] = joysticks_list[i].GetCurrentState();
            }
            PlotModel lfb_model = new PlotModel();
            var test = new ScatterSeries();
            test.MarkerSize = 9;
            test.MarkerType = MarkerType.Circle;

            //var test_vel = new ScatterSeries();
            //test_vel.MarkerSize = 6;
            //test_vel.MarkerType = MarkerType.Cross;

            var vec_x = new LineSeries { Color = OxyColors.Black, StrokeThickness = 1 };
            var vec_y = new LineSeries { Color = OxyColors.Black, StrokeThickness = 1 };


            //var lfb_line_high = new ThreeColorLineSeries {Color = OxyColors.Green, ColorHi = OxyColors.OrangeRed, ColorLo = OxyColors.Blue, LimitHi = 1.02, LimitLo = 0.98};
            //var lfb_line_low = new ThreeColorLineSeries { Color = OxyColors.Green, ColorHi = OxyColors.OrangeRed, ColorLo = OxyColors.Blue, LimitHi = 1.02, LimitLo = 0.98};

            var lfb_line_high = new LineSeries { Color =  OxyColors.Red, StrokeThickness = 4};
            var lfb_line_low = new LineSeries { Color = OxyColors.Green, StrokeThickness = 5 };

            bool lfb_draw = false;

            var xaxis = new OxyPlot.Axes.LinearAxis();
            var yaxis = new OxyPlot.Axes.LinearAxis();

            xaxis.Position = OxyPlot.Axes.AxisPosition.Bottom;
            yaxis.Position = OxyPlot.Axes.AxisPosition.Left;
            xaxis.IsAxisVisible = false;
            yaxis.IsAxisVisible = false;

            var xling = new LineSeries();
            var yline = new LineSeries();
            xling.Color = OxyColors.LightGray;
            yline.Color = OxyColors.LightGray;
            xling.StrokeThickness = 1;
            yline.StrokeThickness = 1;

            xling.Points.Add(new DataPoint(-1, 0));
            xling.Points.Add(new DataPoint(1, 0));
            yline.Points.Add(new DataPoint(0, -1));
            yline.Points.Add(new DataPoint(0, 1));


            lfb_model.Series.Add(lfb_line_high);
            lfb_model.Series.Add(lfb_line_low);

            lfb_model.Series.Add(xling);
            lfb_model.Series.Add(yline);

            lfb_model.Series.Add(DrawModels.CircleLine(1, OxyColors.Black));
            lfb_model.Series.Add(DrawModels.CircleLine(0.75, OxyColors.DimGray));
            lfb_model.Series.Add(DrawModels.CircleLine(0.5, OxyColors.Gray));
            lfb_model.Series.Add(DrawModels.CircleLine(0.25, OxyColors.DarkGray));
            //lfb_model.Annotations.Add(new OxyPlot.Annotations.LineAnnotation { Type =  OxyPlot.Annotations.LineAnnotationType.Horizontal, X=0, Color = OxyColors.DimGray});
            //lfb_model.Annotations.Add(new OxyPlot.Annotations.LineAnnotation { Type = OxyPlot.Annotations.LineAnnotationType.Vertical, Y = 0, Color = OxyColors.Black });
            lfb_model.Series.Add(vec_x);
            lfb_model.Series.Add(vec_y);
            lfb_model.Series.Add(test);


            double acctt = 0;
            double accttp = 0;

            var t2 = new OxyThickness(0,0,0,0);
            lfb_model.PlotAreaBorderThickness = t2;
            lfb_model.Axes.Add(yaxis);
            lfb_model.Axes.Add(xaxis);
            plotView1.Model = lfb_model;

            Braking_Line.Add(new DataPoint(0, 0));

            var speed_angle = Math.Atan2(1, 0);
            Vector speed_vector_rotation = new Vector { X = 0, Y = 1 };


            Vector velocity_normalizde = new Vector { X = 0, Y = 1 };
            Vector velocity_rotated = new Vector { X = 0, Y = 1 };


                List<Form> form = new List<Form>();
                foreach (Form f in Application.OpenForms)
                {
                    form.Add(f);
                }
                for (int i =2; i <= form.Count; i++)
                {
                    form[i-1].Close();
                }

            Form3 f3 = new Form3(Braking_Line);
            f3.Show();
            Form4 f4 = new Form4(Steering_wheel, brake_pedal);
            f4.Show();

            while (Settings_enable == true)
            {
                dtt = DateTime.Now.Ticks;

                try
                {
                    
                    if (throttle_value_nr == 1 && veh_speed < 200/3.6f)
                        veh_speed = 200 / 3.6f;
                    else
                    {
                        if (throttle_value_nr > 0)
                        {
                            accttp = acctt;
                            acctt = DateTime.Now.Ticks;
                            if (accttp>0 && acctt >0)
                            veh_speed = veh_speed + throttle_value_nr * 9.81f * ((float)(acctt - accttp) / 10000000);
                            lfb_line_high.Points.Clear();
                            lfb_line_low.Points.Clear();
                        }
                        else
                        {
                            if (accttp != 0 && acctt != 0)
                            accttp = acctt = 0;
                        }

                    };
                    


                    for (int i = 0; i < joysticks_list.Count; i++)
                    {
                        js_array_test[i] = joysticks_list[i].GetCurrentState();
                    }


                    //Steering Section

                    try
                    {
                        steer_value_nr = CheckAxis.SteerValue_f(steer_axis, steer_id_dv, steer_half, js_array_test);
                        throttle_value_nr = CheckAxis.AxisValue_f(comboBox_axis_throttle.Text, throttle_id_dv, throttle_max, js_array_test);
                        //brake_value_nr = CheckAxis.AxisValue_f(comboBox_axis_brake.Text, brake_id_dv, brake_max, js_array_test);
                        brake_value_nr = avr_brk_pdl(CheckAxis.AxisValue_f(comboBox_axis_brake.Text, brake_id_dv, brake_max, js_array_test));
                    }
                    catch
                    {
                        statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = "Wrong Settings"));
                    }
                    if (checkBox_throttle.Checked == true)
                        throttle_value_nr = 1 - throttle_value_nr ;
                    if (checkBox_brake.Checked == true)
                        brake_value_nr = 1 - brake_value_nr ;
                    var Rc = 2.450f / Math.Tan(steer_value_nr  * 25*Math.PI/180);
                    var An = veh_speed * veh_speed / Rc;
                    //An = An / 9.81;
                    //An = An / 3;


                    Vector2 thr = new Vector2 { Y = throttle_value_nr / 1.50f, X = 0 };
                    Vector2 brk = new Vector2 { Y = -brake_value_nr / 0.85f, X = 0 };
                    //Vector2 thr = new Vector2 { Y = -(float)throttle_value_nr/150, X = 0 };
                    //Vector2 brk = new Vector2 { Y = (float)brake_value_nr/85, X = 0 };
                    //Vector2 steer = new Vector2 { X = (float)steer_value_nr/100, Y = 0 };
                    if (An > 2)
                        An = 2;
                    if (An < -2)
                        An = -2;
                    Vector2 steer = new Vector2 { X = (float)An, Y = 0 };

                    var vall = thr + brk + steer;
                    var vall2 = vall;
                    var va_leght = vall.Length();
                    if (va_leght > 1)
                    {
                        vall = Vector2.Normalize(vall) - vall + Vector2.Normalize(vall);
                    }
                    if (va_leght > 2)
                    {
                        vall.X = 0;
                        vall.Y = 0;
                    }
                    test.Points.Clear();
                    //test_vel.Points.Clear();
                    
                    //vec_x.Points.Clear();
                    //vec_y.Points.Clear();
                    test.Points.Add(new ScatterPoint(vall.X, vall.Y));
                    test.Points.Add(new ScatterPoint(vall2.X, vall2.Y));
                    
                    //vec_x.Points.Add(new DataPoint(vall2.X, 0));
                    //vec_x.Points.Add(new DataPoint(vall2.X, vall2.Y));
                    //vec_y.Points.Add(new DataPoint(0, vall2.Y));
                    //vec_y.Points.Add(new DataPoint(vall2.X, vall2.Y));

                    if (brake_value_nr > 0)
                    {
                        if (lfb_draw == false)
                            lfb_draw = true;
                        else;
                        //dtt = DateTime.Now.Ticks;
                        var dt = ((float)(dtt - dttp) / 10000000);
                        lfb_line_series_low.Add(new DataPoint(vall.X, vall.Y));
                        lfb_line_series_high.Add(new DataPoint((thr + brk + steer).X, (thr + brk + steer).Y));
                        if (Steering_wheel.Count ==0 )
                        Steering_wheel.Add(new DataPoint(0, 1 - Math.Abs(steer_value_nr)));
                        else Steering_wheel.Add(new DataPoint(Steering_wheel.Last().X +dt, 1- Math.Abs(steer_value_nr)));;
                        if (brake_pedal.Count == 0)
                        brake_pedal.Add(new DataPoint(0, Math.Abs(brake_value_nr)));
                        else
                            brake_pedal.Add(new DataPoint(brake_pedal.Last().X + dt, Math.Abs(brake_value_nr)));

                        //statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = speeed_line.X.ToString() + " | "+ speeed_line.Y.ToString() ));

                    }
                    else
                    {
                        dtt = dttp = 0;
                        if (lfb_draw == true)
                        {
                            //asfsfas
                            lfb_draw = false;
                            if (lfb_line_series_high.Count > 50)
                            {
                                lfb_line_high.Points.AddRange(lfb_line_series_high);
                                lfb_line_low.Points.AddRange(lfb_line_series_low);
                                f3.Update(Braking_Line, add_x, add_y, rotation_angle, veh_speed);
                                f4.Update( Steering_wheel, brake_pedal);
                            }
                        lfb_line_series_high.Clear();
                        lfb_line_series_low.Clear();
                        Braking_Line.Clear();
                        Braking_Line.Add(new DataPoint(0, 0));
                        Steering_wheel.Clear();
                        brake_pedal.Clear();
                        speed_angle = Math.Atan2(1, 0);
                            velocity_rotated.X = 0;
                            velocity_rotated.Y = 1;
                            
                        }
                    }


                    plotView1.Model.Axes[0].Zoom(-1.075, 1.075);
                    plotView1.Model.Axes[1].Zoom(-1.075, 1.075);
                    if (va_leght < 0.25)
                        test.MarkerFill = OxyColors.Blue;
                    if (va_leght >= 0.25 && va_leght < 0.5)
                        test.MarkerFill = OxyColors.RoyalBlue;
                    if (va_leght >= 0.5 && va_leght < 0.75)
                        test.MarkerFill = OxyColors.DeepSkyBlue;
                    if (va_leght >= 0.75 && va_leght < 0.9)
                        test.MarkerFill = OxyColors.Yellow;
                    if (va_leght >= 0.9 && va_leght < 0.975)
                            test.MarkerFill = OxyColors.Orange;
                    if (va_leght >= 0.98 && va_leght <= 1.02)
                        test.MarkerFill = OxyColors.Green;
                    //if (va_leght > 1.025 && va_leght <= 1.10)
                    //    test.MarkerFill = OxyColors.DarkOrange;
                    if (va_leght > 1.02 && va_leght <= 1.25)
                        test.MarkerFill = OxyColors.OrangeRed;
                    if (va_leght > 1.10)
                        test.MarkerFill = OxyColors.Red;
                    plotView1.OnModelChanged();
                    var bvc = -brk.Y;

                    if (bvc > 1)
                        bvc = 1 - (brake_value_nr - 1);
                    if (dttp > 0)
                    {
                        var delta_t = ((float)(dtt - dttp) / 10000000);

                        Vector velocity_speed = velocity_normalizde * veh_speed;
                        Vector braking_vector = new Vector { X = vall.X * 9.81f * delta_t, Y = vall.Y * 9.81f * delta_t };
                        Vector velocity_plus_brakig = (new Vector { X = 0, Y = veh_speed }) + braking_vector;

                        rotation_angle = -Math.Atan(vall.X*9.81f * delta_t / veh_speed);

                        //test.Points.Add(new ScatterPoint(braking_vector.X, braking_vector.Y));

                        //test.Points.Add(new ScatterPoint(velocity_rotated.X, velocity_rotated.Y));

                        Vector jump = velocity_rotated * delta_t *veh_speed;
                        add_x = jump.X;
                        add_y = jump.Y;
                        Braking_Line.Add(new DataPoint(Braking_Line.Last().X + add_x, Braking_Line.Last().Y + add_y));

                        //test_vel.Points.Add(new ScatterPoint());

                        //speed_txt.Invoke((MethodInvoker)(() => speed_txt.Text = (rotation_angle * 180/Math.PI).ToString() + " Kph"));

                        speed_angle = speed_angle + rotation_angle;
                        velocity_rotated.X = Math.Cos(speed_angle);
                        velocity_rotated.Y = Math.Sin(speed_angle);


                        veh_speed = veh_speed - bvc * 9.81f * delta_t;
                       // statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = delta_t.ToString() + " | " + velocity_plus_brakig.Length.ToString()));



                    }
                    if (veh_speed < 0)
                        veh_speed = 0;
                    if (veh_speed == 0)
                    {
                        vall.X = 0;
                        vall.Y = 0;
                    }
                    
                    //statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = speeed_line.X.ToString() + " | "+ speeed_line.Y.ToString() ));
                    speed_txt.Invoke((MethodInvoker)(() => speed_txt.Text = ((int)(veh_speed*3.6f)).ToString() + " Kph" ));
                    //speed_txt.Invoke((MethodInvoker)(() => speed_txt.Text = Rc.ToString() + " Kph"));
                    //Text = (steer_value_nr*45).ToString();

                }
                catch
                {
                    //statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = "WORNG SETTINGS"));
                }
                dttp = dtt;
                await Task.Delay(1);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Settings_enable = true;
                foreach (Joystick test in joysticks_list)
                {
                    test.Acquire();
                }
            }
            catch
            {
                Settings_enable = false;
                MessageBox.Show("Wrong settings\nCheck settings and try again");
                foreach (Joystick test in joysticks_list)
                {
                    test.Unacquire();
                }
            }
            Left_foot_brakingAsync();
        }


        // Get Result Button

        private void GetResultButtonClick(object sender, EventArgs e)
        {

            if ((udpclient.Client.IsBound == true | isconnected == true) && totalstate>0)
            {
                isconnected = false;
                acc_telemetry = false;
                udpclient.Close();
                List<float> Time = new List<float>();
                var min_dtt = DateTimeTicks.Min();
                for (int i = 0; i < DateTimeTicks.Count; i++)
                {
                    Time.Add((float)(DateTimeTicks[i] - min_dtt) / 10000000);
                }
                JoystickState str_list = new JoystickState();
                JoystickState thr_list = new JoystickState();
                JoystickState brk_list = new JoystickState();
                JoystickState clt_list = new JoystickState();
                JoystickState hbr_list = new JoystickState();
                JoystickState gup_list = new JoystickState();
                JoystickState gdw_list = new JoystickState();
                float srt_float, thr_float, brk_float, clt_float, hbr_float, gup_float, gdw_float;

                for (int i = 0; i < joystickStates_list_all.Count; i++)
                {

                    str_list = joystickStates_list_all[i][steer_id_dv];
                    thr_list = joystickStates_list_all[i][throttle_id_dv];
                    brk_list = joystickStates_list_all[i][brake_id_dv];
                    clt_list = joystickStates_list_all[i][clutch_id_dv];
                    hbr_list = joystickStates_list_all[i][handbrake_id_dv];
                    gup_list = joystickStates_list_all[i][gearup_id_dv];
                    gdw_list = joystickStates_list_all[i][geardown_id_dv];

                    //Steering Section
                    if (steer_axis.Contains(" "))
                    {
                        var ts = steer_axis.Split(' ');
                        float str_vl = Convert.ToSingle(str_list.Sliders[Convert.ToInt16(ts[1])]);
                        srt_float = (str_vl - (steer_max / 2)) / (steer_max / 2);
                    }
                    else
                    {
                        float str_vl = Convert.ToSingle(str_list.GetType().GetProperty(steer_axis).GetValue(str_list));
                        srt_float = (str_vl - (steer_max / 2)) / (steer_max / 2);
                    }

                    //Throttle Section
                    if (throttle_axis.Contains(" "))
                    {
                        var ts = throttle_axis.Split(' ');
                        float thr_vl = Convert.ToSingle(thr_list.Sliders[Convert.ToInt16(ts[1])]);
                        if (throttle_inverted == true)
                        {
                            thr_float = 1 + (thr_vl / throttle_max) * -1;
                        }
                        else
                        {
                            thr_float = thr_vl / throttle_max;
                        }
                    }
                    else
                    {
                        float thr_vl = Convert.ToSingle(thr_list.GetType().GetProperty(throttle_axis).GetValue(thr_list));
                        if (throttle_inverted == true)
                        {
                            thr_float = 1 + (thr_vl / throttle_max) * -1;
                        }
                        else
                        {
                            thr_float = thr_vl / throttle_max;
                        }
                    }


                    //Brake Section
                    if (brake_axis.Contains(" "))
                    {
                        var ts = brake_axis.Split(' ');
                        float brk_vl = Convert.ToSingle(brk_list.Sliders[Convert.ToInt16(ts[1])]);
                        if (brake_inverted == true)
                        {
                            brk_float = 1 + (brk_vl / brake_max) * -1;
                        }
                        else
                        {
                            brk_float = brk_vl / brake_max;
                        }
                    }
                    else
                    {
                        float brk_vl = Convert.ToSingle(brk_list.GetType().GetProperty(brake_axis).GetValue(brk_list));
                        if (brake_inverted == true)
                        {
                            brk_float = 1 + (brk_vl / brake_max) * -1;
                        }
                        else
                        {
                            brk_float = brk_vl / brake_max;
                        }
                    }

                    //Clutch Section
                    if (clutch_isaxis == true)
                    {
                        if (clutch_axis.Contains(" "))
                        {
                            var ts = clutch_axis.Split(' ');
                            float clt_vl = Convert.ToSingle(clt_list.Sliders[Convert.ToInt16(ts[1])]);
                            if (clutch_inverted == true)
                            {
                                clt_float = 1 + (clt_vl / clutch_max) * -1;
                            }
                            else
                            {
                                clt_float = clt_vl / clutch_max;
                            }
                        }
                        else
                        {
                            float clt_vl = Convert.ToSingle(clt_list.GetType().GetProperty(clutch_axis).GetValue(clt_list));
                            if (clutch_inverted == true)
                            {
                                clt_float = 1 + (clt_vl / clutch_max) * -1;
                            }
                            else
                            {
                                clt_float = clt_vl / clutch_max;
                            }
                        }
                    }
                    else
                    {
                        //var clt_bool = clt_list.Buttons[Convert.ToInt16(clutch_axis.Split(' ')[1])];
                        clt_float = Convert.ToSingle(clt_list.Buttons[Convert.ToInt16(clutch_axis.Split(' ')[1])]);
                    }

                    // Handbrake Section
                    if (handbrake_isaxis == true)
                    {
                        if (handbrake_axis.Contains(" "))
                        {
                            var ts = handbrake_axis.Split(' ');
                            float hbr_vl = Convert.ToSingle(hbr_list.Sliders[Convert.ToInt16(ts[1])]);
                            if (handbrake_inverted == true)
                            {
                                hbr_float = 1 + (hbr_vl / handbrake_max) * -1;
                            }
                            else
                            {
                                hbr_float = hbr_vl / handbrake_max;
                            }
                        }
                        else
                        {
                            float hbr_vl = Convert.ToSingle(hbr_list.GetType().GetProperty(handbrake_axis).GetValue(hbr_list));
                            if (handbrake_inverted == true)
                            {
                                hbr_float = 1 + (hbr_vl / handbrake_max) * -1;
                            }
                            else
                            {
                                hbr_float = hbr_vl / handbrake_max;
                            }
                        }
                    }
                    else
                    {
                        hbr_float = Convert.ToSingle(hbr_list.Buttons[Convert.ToInt16(handbrake_axis.Split(' ')[1])]);
                    }


                    //GearUp and GearDown Section
                    gup_float = Convert.ToSingle(gup_list.Buttons[Convert.ToInt16(gearup_axis.Split(' ')[1])]);
                    gdw_float = Convert.ToSingle(gdw_list.Buttons[Convert.ToInt16(geardown_axis.Split(' ')[1])]);

                    inputlist.Add(new List<float> { srt_float, thr_float, brk_float,clt_float, hbr_float, gup_float, gdw_float });

                }
                plotView_steer.Model = DrawModels.Latency(Time, inputlist, outputlist, 0, 0);
                plotView_throttle.Model = DrawModels.Latency(Time, inputlist, outputlist, 1, 1);
                plotView_brake.Model = DrawModels.Latency(Time, inputlist, outputlist, 2, 2);
                plotView_clutch.Model = DrawModels.Latency(Time, inputlist, outputlist, 3, 3);
                plotView_handbrake.Model = DrawModels.Latency(Time, inputlist, outputlist, 4, 4);
                plotView_gear.Model = DrawModels.Latency(Time, inputlist, outputlist, 5, 6, 5);

                statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = "Telemetry Frequency: " + HzCalc.GetHz(Time) + "Hz"));
                udpclient = new UdpClient();
                tabControl1.SelectTab(tabPage1);
            }
            else
            {
                isconnected = false;
                acc_telemetry = false;
                udpclient.Close();
                statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = "Telemetry NOT FOUND"));
                udpclient = new UdpClient();
            }

        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.plotView_steer = new OxyPlot.WindowsForms.PlotView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.plotView_throttle = new OxyPlot.WindowsForms.PlotView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.plotView_brake = new OxyPlot.WindowsForms.PlotView();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.plotView_clutch = new OxyPlot.WindowsForms.PlotView();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.plotView_handbrake = new OxyPlot.WindowsForms.PlotView();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.plotView_gear = new OxyPlot.WindowsForms.PlotView();
            this.tabPage7 = new System.Windows.Forms.TabPage();
            this.fh_udp_tb = new System.Windows.Forms.MaskedTextBox();
            this.dr_udp_tb = new System.Windows.Forms.MaskedTextBox();
            this.rbr_udp_tb = new System.Windows.Forms.MaskedTextBox();
            this.lfs_udp_tb = new System.Windows.Forms.MaskedTextBox();
            this.ac_delay = new System.Windows.Forms.MaskedTextBox();
            this.export_mp = new System.Windows.Forms.MaskedTextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.DeviceLatencyTester = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.button_savesettings = new System.Windows.Forms.Button();
            this.checkBox_handbrake_is_axis = new System.Windows.Forms.CheckBox();
            this.checkBox_clutch_is_axis = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox_axis_gearup = new System.Windows.Forms.ComboBox();
            this.cb_device_gearup = new System.Windows.Forms.ComboBox();
            this.comboBox_axis_geardown = new System.Windows.Forms.ComboBox();
            this.cb_device_geardown = new System.Windows.Forms.ComboBox();
            this.trackBar_Handbrake = new System.Windows.Forms.TrackBar();
            this.checkBox_handbrake = new System.Windows.Forms.CheckBox();
            this.comboBox_axis_handbrake = new System.Windows.Forms.ComboBox();
            this.cb_device_handbrake = new System.Windows.Forms.ComboBox();
            this.trackBar_clutch = new System.Windows.Forms.TrackBar();
            this.checkBox_clutch = new System.Windows.Forms.CheckBox();
            this.comboBox_axis_clutch = new System.Windows.Forms.ComboBox();
            this.cb_device_clutch = new System.Windows.Forms.ComboBox();
            this.trackBar_brake = new System.Windows.Forms.TrackBar();
            this.checkBox_brake = new System.Windows.Forms.CheckBox();
            this.comboBox_axis_brake = new System.Windows.Forms.ComboBox();
            this.cb_device_brake = new System.Windows.Forms.ComboBox();
            this.trackBar_throttle = new System.Windows.Forms.TrackBar();
            this.checkBox_throttle = new System.Windows.Forms.CheckBox();
            this.comboBox_axis_throttle = new System.Windows.Forms.ComboBox();
            this.cb_device_throttle = new System.Windows.Forms.ComboBox();
            this.trackBar_steering = new System.Windows.Forms.TrackBar();
            this.checkBox_steer = new System.Windows.Forms.CheckBox();
            this.comboBox_axis_steering = new System.Windows.Forms.ComboBox();
            this.cb_device_steer = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.RequestDeviceData = new System.Windows.Forms.Button();
            this.GetDevices = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.tabPage8 = new System.Windows.Forms.TabPage();
            this.speed_txt = new System.Windows.Forms.Label();
            this.plotView1 = new OxyPlot.WindowsForms.PlotView();
            this.LfsTelemetryButton = new System.Windows.Forms.Button();
            this.GetResultButton = new System.Windows.Forms.Button();
            this.RbrTelemetryButton = new System.Windows.Forms.Button();
            this.DirtRallyButton = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.acc_telemetry_button = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.tabPage7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_Handbrake)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_clutch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_brake)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_throttle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_steering)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tabPage8.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 452);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1359, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Controls.Add(this.tabPage6);
            this.tabControl1.Controls.Add(this.tabPage7);
            this.tabControl1.Controls.Add(this.tabPage8);
            this.tabControl1.Location = new System.Drawing.Point(0, 39);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1359, 410);
            this.tabControl1.TabIndex = 1;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.plotView_steer);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1351, 384);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Steer";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // plotView_steer
            // 
            this.plotView_steer.AccessibleName = "";
            this.plotView_steer.Location = new System.Drawing.Point(0, 0);
            this.plotView_steer.Name = "plotView_steer";
            this.plotView_steer.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotView_steer.Size = new System.Drawing.Size(1351, 379);
            this.plotView_steer.TabIndex = 0;
            this.plotView_steer.Text = "plotView1";
            this.plotView_steer.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotView_steer.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotView_steer.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.plotView_throttle);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1351, 384);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Throttle";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // plotView_throttle
            // 
            this.plotView_throttle.Location = new System.Drawing.Point(0, 0);
            this.plotView_throttle.Name = "plotView_throttle";
            this.plotView_throttle.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotView_throttle.Size = new System.Drawing.Size(1351, 379);
            this.plotView_throttle.TabIndex = 1;
            this.plotView_throttle.Text = "plotView1";
            this.plotView_throttle.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotView_throttle.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotView_throttle.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.plotView_brake);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(1351, 384);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Brake";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // plotView_brake
            // 
            this.plotView_brake.Location = new System.Drawing.Point(0, 0);
            this.plotView_brake.Name = "plotView_brake";
            this.plotView_brake.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotView_brake.Size = new System.Drawing.Size(1351, 379);
            this.plotView_brake.TabIndex = 1;
            this.plotView_brake.Text = "plotView_brake";
            this.plotView_brake.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotView_brake.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotView_brake.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.plotView_clutch);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(1351, 384);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Clutch";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // plotView_clutch
            // 
            this.plotView_clutch.Location = new System.Drawing.Point(0, 0);
            this.plotView_clutch.Name = "plotView_clutch";
            this.plotView_clutch.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotView_clutch.Size = new System.Drawing.Size(1351, 379);
            this.plotView_clutch.TabIndex = 1;
            this.plotView_clutch.Text = "plotView_clutch";
            this.plotView_clutch.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotView_clutch.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotView_clutch.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.plotView_handbrake);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(1351, 384);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Handbrake";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // plotView_handbrake
            // 
            this.plotView_handbrake.Location = new System.Drawing.Point(0, 0);
            this.plotView_handbrake.Name = "plotView_handbrake";
            this.plotView_handbrake.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotView_handbrake.Size = new System.Drawing.Size(1351, 379);
            this.plotView_handbrake.TabIndex = 1;
            this.plotView_handbrake.Text = "plotView_handbrake";
            this.plotView_handbrake.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotView_handbrake.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotView_handbrake.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.plotView_gear);
            this.tabPage6.Location = new System.Drawing.Point(4, 22);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Size = new System.Drawing.Size(1351, 384);
            this.tabPage6.TabIndex = 5;
            this.tabPage6.Text = "Gear";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // plotView_gear
            // 
            this.plotView_gear.Location = new System.Drawing.Point(0, 0);
            this.plotView_gear.Name = "plotView_gear";
            this.plotView_gear.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotView_gear.Size = new System.Drawing.Size(1351, 379);
            this.plotView_gear.TabIndex = 1;
            this.plotView_gear.Text = "plotView_gear";
            this.plotView_gear.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotView_gear.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotView_gear.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // tabPage7
            // 
            this.tabPage7.Controls.Add(this.fh_udp_tb);
            this.tabPage7.Controls.Add(this.dr_udp_tb);
            this.tabPage7.Controls.Add(this.rbr_udp_tb);
            this.tabPage7.Controls.Add(this.lfs_udp_tb);
            this.tabPage7.Controls.Add(this.ac_delay);
            this.tabPage7.Controls.Add(this.export_mp);
            this.tabPage7.Controls.Add(this.label10);
            this.tabPage7.Controls.Add(this.DeviceLatencyTester);
            this.tabPage7.Controls.Add(this.label9);
            this.tabPage7.Controls.Add(this.label7);
            this.tabPage7.Controls.Add(this.label8);
            this.tabPage7.Controls.Add(this.label2);
            this.tabPage7.Controls.Add(this.label1);
            this.tabPage7.Controls.Add(this.label6);
            this.tabPage7.Controls.Add(this.button_savesettings);
            this.tabPage7.Controls.Add(this.checkBox_handbrake_is_axis);
            this.tabPage7.Controls.Add(this.checkBox_clutch_is_axis);
            this.tabPage7.Controls.Add(this.label5);
            this.tabPage7.Controls.Add(this.label4);
            this.tabPage7.Controls.Add(this.comboBox_axis_gearup);
            this.tabPage7.Controls.Add(this.cb_device_gearup);
            this.tabPage7.Controls.Add(this.comboBox_axis_geardown);
            this.tabPage7.Controls.Add(this.cb_device_geardown);
            this.tabPage7.Controls.Add(this.trackBar_Handbrake);
            this.tabPage7.Controls.Add(this.checkBox_handbrake);
            this.tabPage7.Controls.Add(this.comboBox_axis_handbrake);
            this.tabPage7.Controls.Add(this.cb_device_handbrake);
            this.tabPage7.Controls.Add(this.trackBar_clutch);
            this.tabPage7.Controls.Add(this.checkBox_clutch);
            this.tabPage7.Controls.Add(this.comboBox_axis_clutch);
            this.tabPage7.Controls.Add(this.cb_device_clutch);
            this.tabPage7.Controls.Add(this.trackBar_brake);
            this.tabPage7.Controls.Add(this.checkBox_brake);
            this.tabPage7.Controls.Add(this.comboBox_axis_brake);
            this.tabPage7.Controls.Add(this.cb_device_brake);
            this.tabPage7.Controls.Add(this.trackBar_throttle);
            this.tabPage7.Controls.Add(this.checkBox_throttle);
            this.tabPage7.Controls.Add(this.comboBox_axis_throttle);
            this.tabPage7.Controls.Add(this.cb_device_throttle);
            this.tabPage7.Controls.Add(this.trackBar_steering);
            this.tabPage7.Controls.Add(this.checkBox_steer);
            this.tabPage7.Controls.Add(this.comboBox_axis_steering);
            this.tabPage7.Controls.Add(this.cb_device_steer);
            this.tabPage7.Controls.Add(this.label3);
            this.tabPage7.Controls.Add(this.RequestDeviceData);
            this.tabPage7.Controls.Add(this.GetDevices);
            this.tabPage7.Controls.Add(this.dataGridView1);
            this.tabPage7.Location = new System.Drawing.Point(4, 22);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Size = new System.Drawing.Size(1351, 384);
            this.tabPage7.TabIndex = 6;
            this.tabPage7.Text = "Settings";
            this.tabPage7.UseVisualStyleBackColor = true;
            // 
            // fh_udp_tb
            // 
            this.fh_udp_tb.Location = new System.Drawing.Point(174, 348);
            this.fh_udp_tb.Mask = "09999";
            this.fh_udp_tb.Name = "fh_udp_tb";
            this.fh_udp_tb.Size = new System.Drawing.Size(55, 20);
            this.fh_udp_tb.TabIndex = 61;
            this.fh_udp_tb.Text = "8000";
            // 
            // dr_udp_tb
            // 
            this.dr_udp_tb.Location = new System.Drawing.Point(174, 323);
            this.dr_udp_tb.Mask = "09999";
            this.dr_udp_tb.Name = "dr_udp_tb";
            this.dr_udp_tb.Size = new System.Drawing.Size(55, 20);
            this.dr_udp_tb.TabIndex = 60;
            this.dr_udp_tb.Text = "20777";
            // 
            // rbr_udp_tb
            // 
            this.rbr_udp_tb.Location = new System.Drawing.Point(174, 294);
            this.rbr_udp_tb.Mask = "09999";
            this.rbr_udp_tb.Name = "rbr_udp_tb";
            this.rbr_udp_tb.Size = new System.Drawing.Size(55, 20);
            this.rbr_udp_tb.TabIndex = 59;
            this.rbr_udp_tb.Text = "6776";
            // 
            // lfs_udp_tb
            // 
            this.lfs_udp_tb.Location = new System.Drawing.Point(174, 265);
            this.lfs_udp_tb.Mask = "09999";
            this.lfs_udp_tb.Name = "lfs_udp_tb";
            this.lfs_udp_tb.Size = new System.Drawing.Size(55, 20);
            this.lfs_udp_tb.TabIndex = 58;
            this.lfs_udp_tb.Text = "63393";
            // 
            // ac_delay
            // 
            this.ac_delay.Location = new System.Drawing.Point(357, 265);
            this.ac_delay.Mask = "09";
            this.ac_delay.Name = "ac_delay";
            this.ac_delay.Size = new System.Drawing.Size(39, 20);
            this.ac_delay.TabIndex = 57;
            this.ac_delay.Text = "16";
            this.ac_delay.TextChanged += new System.EventHandler(this.ac_delay_TextChanged);
            // 
            // export_mp
            // 
            this.export_mp.Location = new System.Drawing.Point(357, 294);
            this.export_mp.Mask = "0.9";
            this.export_mp.Name = "export_mp";
            this.export_mp.Size = new System.Drawing.Size(39, 20);
            this.export_mp.TabIndex = 56;
            this.export_mp.Text = "10";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(250, 297);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(89, 13);
            this.label10.TabIndex = 54;
            this.label10.Text = "Export mupltiplier:";
            // 
            // DeviceLatencyTester
            // 
            this.DeviceLatencyTester.Location = new System.Drawing.Point(339, 342);
            this.DeviceLatencyTester.Name = "DeviceLatencyTester";
            this.DeviceLatencyTester.Size = new System.Drawing.Size(118, 29);
            this.DeviceLatencyTester.TabIndex = 53;
            this.DeviceLatencyTester.Text = "Device Latency Test";
            this.DeviceLatencyTester.UseVisualStyleBackColor = true;
            this.DeviceLatencyTester.Click += new System.EventHandler(this.DeviceLatencyTester_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(405, 260);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(117, 26);
            this.label9.TabIndex = 52;
            this.label9.Text = "20 - 50Hz    10 - 100Hz\r\n5 - 200Hz      2 - 400Hz\r\n";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(250, 268);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(103, 13);
            this.label7.TabIndex = 51;
            this.label7.Text = "AC Telemetry Delay:\r\n";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(26, 351);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(121, 13);
            this.label8.TabIndex = 48;
            this.label8.Text = "Forza Horizon 4\\5 UDP:\r\n";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 326);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 46;
            this.label2.Text = "Dirt Rally UDP";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 297);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 13);
            this.label1.TabIndex = 45;
            this.label1.Text = "Richard Burns Rally UDP:\r\n";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(26, 268);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(108, 13);
            this.label6.TabIndex = 39;
            this.label6.Text = "Live For Speed UDP:\r\n";
            // 
            // button_savesettings
            // 
            this.button_savesettings.Location = new System.Drawing.Point(408, 217);
            this.button_savesettings.Name = "button_savesettings";
            this.button_savesettings.Size = new System.Drawing.Size(118, 29);
            this.button_savesettings.TabIndex = 37;
            this.button_savesettings.Text = "Save Setting";
            this.button_savesettings.UseVisualStyleBackColor = true;
            this.button_savesettings.Click += new System.EventHandler(this.button_savesettings_Click);
            // 
            // checkBox_handbrake_is_axis
            // 
            this.checkBox_handbrake_is_axis.AutoSize = true;
            this.checkBox_handbrake_is_axis.Checked = true;
            this.checkBox_handbrake_is_axis.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_handbrake_is_axis.Location = new System.Drawing.Point(554, 286);
            this.checkBox_handbrake_is_axis.Name = "checkBox_handbrake_is_axis";
            this.checkBox_handbrake_is_axis.Size = new System.Drawing.Size(45, 17);
            this.checkBox_handbrake_is_axis.TabIndex = 36;
            this.checkBox_handbrake_is_axis.Text = "Axis";
            this.checkBox_handbrake_is_axis.UseVisualStyleBackColor = true;
            this.checkBox_handbrake_is_axis.CheckedChanged += new System.EventHandler(this.checkBox_handbrake_is_axis_CheckedChanged);
            // 
            // checkBox_clutch_is_axis
            // 
            this.checkBox_clutch_is_axis.AutoSize = true;
            this.checkBox_clutch_is_axis.Checked = true;
            this.checkBox_clutch_is_axis.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_clutch_is_axis.Location = new System.Drawing.Point(554, 223);
            this.checkBox_clutch_is_axis.Name = "checkBox_clutch_is_axis";
            this.checkBox_clutch_is_axis.Size = new System.Drawing.Size(45, 17);
            this.checkBox_clutch_is_axis.TabIndex = 35;
            this.checkBox_clutch_is_axis.Text = "Axis";
            this.checkBox_clutch_is_axis.UseVisualStyleBackColor = true;
            this.checkBox_clutch_is_axis.CheckedChanged += new System.EventHandler(this.checkBox_clutch_is_axis_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(682, 6);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(252, 13);
            this.label5.TabIndex = 33;
            this.label5.Text = "Device                              Axis/Button         Inverted";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Segoe UI", 27.75F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(944, 313);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(399, 50);
            this.label4.TabIndex = 32;
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // comboBox_axis_gearup
            // 
            this.comboBox_axis_gearup.FormattingEnabled = true;
            this.comboBox_axis_gearup.Location = new System.Drawing.Point(778, 318);
            this.comboBox_axis_gearup.Name = "comboBox_axis_gearup";
            this.comboBox_axis_gearup.Size = new System.Drawing.Size(121, 21);
            this.comboBox_axis_gearup.TabIndex = 31;
            // 
            // cb_device_gearup
            // 
            this.cb_device_gearup.FormattingEnabled = true;
            this.cb_device_gearup.Location = new System.Drawing.Point(642, 318);
            this.cb_device_gearup.Name = "cb_device_gearup";
            this.cb_device_gearup.Size = new System.Drawing.Size(121, 21);
            this.cb_device_gearup.TabIndex = 30;
            // 
            // comboBox_axis_geardown
            // 
            this.comboBox_axis_geardown.FormattingEnabled = true;
            this.comboBox_axis_geardown.Location = new System.Drawing.Point(778, 347);
            this.comboBox_axis_geardown.Name = "comboBox_axis_geardown";
            this.comboBox_axis_geardown.Size = new System.Drawing.Size(121, 21);
            this.comboBox_axis_geardown.TabIndex = 29;
            // 
            // cb_device_geardown
            // 
            this.cb_device_geardown.FormattingEnabled = true;
            this.cb_device_geardown.Location = new System.Drawing.Point(642, 347);
            this.cb_device_geardown.Name = "cb_device_geardown";
            this.cb_device_geardown.Size = new System.Drawing.Size(121, 21);
            this.cb_device_geardown.TabIndex = 28;
            // 
            // trackBar_Handbrake
            // 
            this.trackBar_Handbrake.AutoSize = false;
            this.trackBar_Handbrake.LargeChange = 0;
            this.trackBar_Handbrake.Location = new System.Drawing.Point(944, 260);
            this.trackBar_Handbrake.Maximum = 100;
            this.trackBar_Handbrake.Name = "trackBar_Handbrake";
            this.trackBar_Handbrake.Size = new System.Drawing.Size(399, 34);
            this.trackBar_Handbrake.SmallChange = 0;
            this.trackBar_Handbrake.TabIndex = 27;
            this.trackBar_Handbrake.TickFrequency = 10;
            // 
            // checkBox_handbrake
            // 
            this.checkBox_handbrake.AutoSize = true;
            this.checkBox_handbrake.Location = new System.Drawing.Point(915, 264);
            this.checkBox_handbrake.Name = "checkBox_handbrake";
            this.checkBox_handbrake.Size = new System.Drawing.Size(15, 14);
            this.checkBox_handbrake.TabIndex = 26;
            this.checkBox_handbrake.UseVisualStyleBackColor = true;
            // 
            // comboBox_axis_handbrake
            // 
            this.comboBox_axis_handbrake.FormattingEnabled = true;
            this.comboBox_axis_handbrake.Location = new System.Drawing.Point(778, 260);
            this.comboBox_axis_handbrake.Name = "comboBox_axis_handbrake";
            this.comboBox_axis_handbrake.Size = new System.Drawing.Size(121, 21);
            this.comboBox_axis_handbrake.TabIndex = 25;
            // 
            // cb_device_handbrake
            // 
            this.cb_device_handbrake.FormattingEnabled = true;
            this.cb_device_handbrake.Location = new System.Drawing.Point(642, 260);
            this.cb_device_handbrake.Name = "cb_device_handbrake";
            this.cb_device_handbrake.Size = new System.Drawing.Size(121, 21);
            this.cb_device_handbrake.TabIndex = 24;
            // 
            // trackBar_clutch
            // 
            this.trackBar_clutch.AutoSize = false;
            this.trackBar_clutch.LargeChange = 0;
            this.trackBar_clutch.Location = new System.Drawing.Point(944, 203);
            this.trackBar_clutch.Maximum = 100;
            this.trackBar_clutch.Name = "trackBar_clutch";
            this.trackBar_clutch.Size = new System.Drawing.Size(399, 34);
            this.trackBar_clutch.TabIndex = 23;
            this.trackBar_clutch.TickFrequency = 10;
            // 
            // checkBox_clutch
            // 
            this.checkBox_clutch.AutoSize = true;
            this.checkBox_clutch.Location = new System.Drawing.Point(915, 207);
            this.checkBox_clutch.Name = "checkBox_clutch";
            this.checkBox_clutch.Size = new System.Drawing.Size(15, 14);
            this.checkBox_clutch.TabIndex = 22;
            this.checkBox_clutch.UseVisualStyleBackColor = true;
            // 
            // comboBox_axis_clutch
            // 
            this.comboBox_axis_clutch.FormattingEnabled = true;
            this.comboBox_axis_clutch.Location = new System.Drawing.Point(778, 203);
            this.comboBox_axis_clutch.Name = "comboBox_axis_clutch";
            this.comboBox_axis_clutch.Size = new System.Drawing.Size(121, 21);
            this.comboBox_axis_clutch.TabIndex = 21;
            // 
            // cb_device_clutch
            // 
            this.cb_device_clutch.FormattingEnabled = true;
            this.cb_device_clutch.Location = new System.Drawing.Point(642, 203);
            this.cb_device_clutch.Name = "cb_device_clutch";
            this.cb_device_clutch.Size = new System.Drawing.Size(121, 21);
            this.cb_device_clutch.TabIndex = 20;
            // 
            // trackBar_brake
            // 
            this.trackBar_brake.AutoSize = false;
            this.trackBar_brake.LargeChange = 1;
            this.trackBar_brake.Location = new System.Drawing.Point(944, 142);
            this.trackBar_brake.Maximum = 100;
            this.trackBar_brake.Name = "trackBar_brake";
            this.trackBar_brake.Size = new System.Drawing.Size(399, 34);
            this.trackBar_brake.TabIndex = 19;
            this.trackBar_brake.TickFrequency = 10;
            // 
            // checkBox_brake
            // 
            this.checkBox_brake.AutoSize = true;
            this.checkBox_brake.Location = new System.Drawing.Point(915, 146);
            this.checkBox_brake.Name = "checkBox_brake";
            this.checkBox_brake.Size = new System.Drawing.Size(15, 14);
            this.checkBox_brake.TabIndex = 18;
            this.checkBox_brake.UseVisualStyleBackColor = true;
            // 
            // comboBox_axis_brake
            // 
            this.comboBox_axis_brake.FormattingEnabled = true;
            this.comboBox_axis_brake.Location = new System.Drawing.Point(778, 142);
            this.comboBox_axis_brake.Name = "comboBox_axis_brake";
            this.comboBox_axis_brake.Size = new System.Drawing.Size(121, 21);
            this.comboBox_axis_brake.TabIndex = 17;
            // 
            // cb_device_brake
            // 
            this.cb_device_brake.FormattingEnabled = true;
            this.cb_device_brake.Location = new System.Drawing.Point(642, 142);
            this.cb_device_brake.Name = "cb_device_brake";
            this.cb_device_brake.Size = new System.Drawing.Size(121, 21);
            this.cb_device_brake.TabIndex = 16;
            // 
            // trackBar_throttle
            // 
            this.trackBar_throttle.AutoSize = false;
            this.trackBar_throttle.LargeChange = 0;
            this.trackBar_throttle.Location = new System.Drawing.Point(944, 83);
            this.trackBar_throttle.Maximum = 100;
            this.trackBar_throttle.Name = "trackBar_throttle";
            this.trackBar_throttle.Size = new System.Drawing.Size(399, 34);
            this.trackBar_throttle.SmallChange = 0;
            this.trackBar_throttle.TabIndex = 15;
            this.trackBar_throttle.TickFrequency = 10;
            // 
            // checkBox_throttle
            // 
            this.checkBox_throttle.AutoSize = true;
            this.checkBox_throttle.Location = new System.Drawing.Point(915, 87);
            this.checkBox_throttle.Name = "checkBox_throttle";
            this.checkBox_throttle.Size = new System.Drawing.Size(15, 14);
            this.checkBox_throttle.TabIndex = 14;
            this.checkBox_throttle.UseVisualStyleBackColor = true;
            // 
            // comboBox_axis_throttle
            // 
            this.comboBox_axis_throttle.FormattingEnabled = true;
            this.comboBox_axis_throttle.Location = new System.Drawing.Point(778, 83);
            this.comboBox_axis_throttle.Name = "comboBox_axis_throttle";
            this.comboBox_axis_throttle.Size = new System.Drawing.Size(121, 21);
            this.comboBox_axis_throttle.TabIndex = 13;
            // 
            // cb_device_throttle
            // 
            this.cb_device_throttle.FormattingEnabled = true;
            this.cb_device_throttle.Location = new System.Drawing.Point(642, 83);
            this.cb_device_throttle.Name = "cb_device_throttle";
            this.cb_device_throttle.Size = new System.Drawing.Size(121, 21);
            this.cb_device_throttle.TabIndex = 12;
            // 
            // trackBar_steering
            // 
            this.trackBar_steering.AutoSize = false;
            this.trackBar_steering.LargeChange = 0;
            this.trackBar_steering.Location = new System.Drawing.Point(944, 24);
            this.trackBar_steering.Maximum = 100;
            this.trackBar_steering.Minimum = -100;
            this.trackBar_steering.Name = "trackBar_steering";
            this.trackBar_steering.Size = new System.Drawing.Size(399, 34);
            this.trackBar_steering.SmallChange = 0;
            this.trackBar_steering.TabIndex = 11;
            this.trackBar_steering.TickFrequency = 20;
            // 
            // checkBox_steer
            // 
            this.checkBox_steer.AutoSize = true;
            this.checkBox_steer.Location = new System.Drawing.Point(915, 28);
            this.checkBox_steer.Name = "checkBox_steer";
            this.checkBox_steer.Size = new System.Drawing.Size(15, 14);
            this.checkBox_steer.TabIndex = 10;
            this.checkBox_steer.UseVisualStyleBackColor = true;
            this.checkBox_steer.Visible = false;
            // 
            // comboBox_axis_steering
            // 
            this.comboBox_axis_steering.FormattingEnabled = true;
            this.comboBox_axis_steering.Location = new System.Drawing.Point(778, 24);
            this.comboBox_axis_steering.Name = "comboBox_axis_steering";
            this.comboBox_axis_steering.Size = new System.Drawing.Size(121, 21);
            this.comboBox_axis_steering.TabIndex = 9;
            // 
            // cb_device_steer
            // 
            this.cb_device_steer.FormattingEnabled = true;
            this.cb_device_steer.Location = new System.Drawing.Point(642, 24);
            this.cb_device_steer.Name = "cb_device_steer";
            this.cb_device_steer.Size = new System.Drawing.Size(121, 21);
            this.cb_device_steer.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(545, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(91, 340);
            this.label3.TabIndex = 6;
            this.label3.Text = "Steering\r\n\r\n\r\nThrottle\r\n\r\n\r\nBrake\r\n\r\n\r\nClutch\r\n\r\n\r\nHandbrake\r\n\r\n\r\nGearUP\r\nGearDOW" +
    "N";
            // 
            // RequestDeviceData
            // 
            this.RequestDeviceData.Location = new System.Drawing.Point(213, 217);
            this.RequestDeviceData.Name = "RequestDeviceData";
            this.RequestDeviceData.Size = new System.Drawing.Size(118, 29);
            this.RequestDeviceData.TabIndex = 3;
            this.RequestDeviceData.Text = "Request Device";
            this.RequestDeviceData.UseVisualStyleBackColor = true;
            this.RequestDeviceData.Click += new System.EventHandler(this.RequestDeviceData_Click);
            // 
            // GetDevices
            // 
            this.GetDevices.Location = new System.Drawing.Point(3, 217);
            this.GetDevices.Name = "GetDevices";
            this.GetDevices.Size = new System.Drawing.Size(118, 29);
            this.GetDevices.TabIndex = 2;
            this.GetDevices.Text = "Refresh";
            this.GetDevices.UseVisualStyleBackColor = true;
            this.GetDevices.Click += new System.EventHandler(this.GetDevices_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(3, 3);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 29;
            this.dataGridView1.Size = new System.Drawing.Size(523, 199);
            this.dataGridView1.TabIndex = 0;
            // 
            // tabPage8
            // 
            this.tabPage8.Controls.Add(this.speed_txt);
            this.tabPage8.Controls.Add(this.plotView1);
            this.tabPage8.Location = new System.Drawing.Point(4, 22);
            this.tabPage8.Name = "tabPage8";
            this.tabPage8.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage8.Size = new System.Drawing.Size(1351, 384);
            this.tabPage8.TabIndex = 7;
            this.tabPage8.Text = "Left Foot Braking";
            this.tabPage8.UseVisualStyleBackColor = true;
            // 
            // speed_txt
            // 
            this.speed_txt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.speed_txt.AutoSize = true;
            this.speed_txt.BackColor = System.Drawing.SystemColors.Control;
            this.speed_txt.Font = new System.Drawing.Font("Segoe UI", 27.75F, System.Drawing.FontStyle.Bold);
            this.speed_txt.ImageAlign = System.Drawing.ContentAlignment.TopRight;
            this.speed_txt.Location = new System.Drawing.Point(595, 3);
            this.speed_txt.Name = "speed_txt";
            this.speed_txt.Size = new System.Drawing.Size(164, 50);
            this.speed_txt.TabIndex = 33;
            this.speed_txt.Text = "200 Kph";
            this.speed_txt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // plotView1
            // 
            this.plotView1.Location = new System.Drawing.Point(0, 0);
            this.plotView1.Name = "plotView1";
            this.plotView1.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotView1.Size = new System.Drawing.Size(762, 762);
            this.plotView1.TabIndex = 2;
            this.plotView1.Text = "plotView1";
            this.plotView1.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotView1.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotView1.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // LfsTelemetryButton
            // 
            this.LfsTelemetryButton.Location = new System.Drawing.Point(6, 6);
            this.LfsTelemetryButton.Name = "LfsTelemetryButton";
            this.LfsTelemetryButton.Size = new System.Drawing.Size(94, 29);
            this.LfsTelemetryButton.TabIndex = 1;
            this.LfsTelemetryButton.Text = "Live For Speed";
            this.LfsTelemetryButton.UseVisualStyleBackColor = true;
            this.LfsTelemetryButton.Click += new System.EventHandler(this.LFSTelemetryButtonClick);
            // 
            // GetResultButton
            // 
            this.GetResultButton.Location = new System.Drawing.Point(1153, 6);
            this.GetResultButton.Name = "GetResultButton";
            this.GetResultButton.Size = new System.Drawing.Size(94, 29);
            this.GetResultButton.TabIndex = 2;
            this.GetResultButton.Text = "Get Result";
            this.GetResultButton.UseVisualStyleBackColor = true;
            this.GetResultButton.Click += new System.EventHandler(this.GetResultButtonClick);
            // 
            // RbrTelemetryButton
            // 
            this.RbrTelemetryButton.Location = new System.Drawing.Point(106, 6);
            this.RbrTelemetryButton.Name = "RbrTelemetryButton";
            this.RbrTelemetryButton.Size = new System.Drawing.Size(94, 29);
            this.RbrTelemetryButton.TabIndex = 3;
            this.RbrTelemetryButton.Text = "RBR NGP";
            this.RbrTelemetryButton.UseVisualStyleBackColor = true;
            this.RbrTelemetryButton.Click += new System.EventHandler(this.RBR_Telemetry_Button_Click);
            // 
            // DirtRallyButton
            // 
            this.DirtRallyButton.Location = new System.Drawing.Point(206, 6);
            this.DirtRallyButton.Name = "DirtRallyButton";
            this.DirtRallyButton.Size = new System.Drawing.Size(94, 29);
            this.DirtRallyButton.TabIndex = 4;
            this.DirtRallyButton.Text = "Dirt Rally";
            this.DirtRallyButton.UseVisualStyleBackColor = true;
            this.DirtRallyButton.Click += new System.EventHandler(this.Dirt_Rally_Button_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(306, 6);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(94, 29);
            this.button3.TabIndex = 6;
            this.button3.Text = "Forza Horizon";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.Forza_Horizon_Button_Click);
            // 
            // acc_telemetry_button
            // 
            this.acc_telemetry_button.Location = new System.Drawing.Point(406, 6);
            this.acc_telemetry_button.Name = "acc_telemetry_button";
            this.acc_telemetry_button.Size = new System.Drawing.Size(94, 29);
            this.acc_telemetry_button.TabIndex = 7;
            this.acc_telemetry_button.Text = "Assetto Corsa";
            this.acc_telemetry_button.UseVisualStyleBackColor = true;
            this.acc_telemetry_button.Click += new System.EventHandler(this.acc_telemetry_button_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1253, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(94, 29);
            this.button1.TabIndex = 8;
            this.button1.Text = "Export to png";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AccessibleName = "";
            this.ClientSize = new System.Drawing.Size(1359, 474);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.acc_telemetry_button);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.DirtRallyButton);
            this.Controls.Add(this.RbrTelemetryButton);
            this.Controls.Add(this.GetResultButton);
            this.Controls.Add(this.LfsTelemetryButton);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Device Latency and FFB Tester";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.tabPage6.ResumeLayout(false);
            this.tabPage7.ResumeLayout(false);
            this.tabPage7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_Handbrake)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_clutch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_brake)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_throttle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_steering)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tabPage8.ResumeLayout(false);
            this.tabPage8.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            deviceslist = directinput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
            statusStrip1.Invoke((MethodInvoker)(() => statusStrip1.Items[0].Text = deviceslist.Count.ToString() + " Devices Found"));
            List<int> button_cap = new List<int>();
            foreach (DeviceInstance di in deviceslist)
            {
                devices_name.Add(di.InstanceName);
                devices_id.Add(di.InstanceGuid);
                devices_id_str.Add(di.InstanceGuid.ToString());
                joysticks_list.Add(new Joystick(directinput, di.InstanceGuid));
                button_cap.Add(new Joystick(directinput, di.InstanceGuid).Capabilities.ButtonCount);
            }
            device_axis_list.AddRange( new List<object> { "X", "Y", "Z", "RotationX", "RotationY", "RotationZ", "Sliders 0", "Sliders 1" });//, "PointOfViewControllers: 0", "PointOfViewControllers: 1", "PointOfViewControllers: 2", "PointOfViewControllers: 3"});
            if (button_cap.Count > 0)
            {
                for (int i = 0; i < button_cap.Max(); i++)
                {
                    device_button_list.Add("button " + i.ToString());
                }
            }
            //device_axis_list.AddRange(new List<object> { "VelocityX", "VelocityY", "VelocityZ", "AngularVelocityX", "AngularVelocityY", "AngularVelocityZ", "VelocitySliders: 0", "VelocitySliders: 1", "AccelerationX", "AccelerationY", "AccelerationZ", "AngularAccelerationX", "AngularAccelerationY", "AngularAccelerationZ", "AccelerationSliders: 0", "AccelerationSliders: 1", "ForceX", "ForceY", "ForceZ", "TorqueX", "TorqueY", "TorqueZ", "ForceSliders: 0", "ForceSliders: 1" });
            cb_device_steer.Items.AddRange(devices_name.ToArray());
            cb_device_throttle.Items.AddRange(devices_name.ToArray());
            cb_device_brake.Items.AddRange(devices_name.ToArray());
            cb_device_clutch.Items.AddRange(devices_name.ToArray());
            cb_device_handbrake.Items.AddRange(devices_name.ToArray());
            cb_device_gearup.Items.AddRange(devices_name.ToArray());
            cb_device_geardown.Items.AddRange(devices_name.ToArray());
            //device_gags_test.AddRange(new JoystickState().X, new JoystickState().Y, new JoystickState().Z, new JoystickState().RotationX, new JoystickState().RotationY, new JoystickState().RotationZ, new JoystickState().Sliders, new JoystickState().PointOfViewControllers, new JoystickState().Buttons, new JoystickState().VelocityX, new JoystickState().VelocityY, new JoystickState().VelocityZ, new JoystickState().AngularVelocityX, new JoystickState().AngularVelocityY, new JoystickState().AngularVelocityZ, new JoystickState().VelocitySliders, new JoystickState().AccelerationX, new JoystickState().AccelerationY, new JoystickState().AccelerationZ, new JoystickState().AngularAccelerationX, new JoystickState().AngularAccelerationY, new JoystickState().AngularAccelerationZ, new JoystickState().AccelerationSliders, new JoystickState().ForceX, new JoystickState().ForceY, new JoystickState().ForceZ, new JoystickState().TorqueX, new JoystickState().TorqueY, new JoystickState().TorqueZ, new JoystickState().ForceSliders);

            //device_gags_test.AddRange(new JoystickState().ToString().Split(","));



            comboBox_axis_steering.Items.AddRange(device_axis_list.ToArray());
            comboBox_axis_geardown.Items.AddRange(device_button_list.ToArray());
            comboBox_axis_gearup.Items.AddRange(device_button_list.ToArray());
            comboBox_axis_throttle.Items.AddRange(device_axis_list.ToArray());
            comboBox_axis_brake.Items.AddRange(device_axis_list.ToArray());
            //comboBox_axis_clutch.Items.AddRange(device_axis_list.ToArray());
            checkBox_clutch_is_axis_CheckedChanged(sender, null);
            //comboBox_axis_handbrake.Items.AddRange(device_axis_list.ToArray());
            checkBox_handbrake_is_axis_CheckedChanged(sender, null);

            if (Directory.Exists(settingspath) && File.Exists(settingspath + "steering.ini") && File.Exists(settingspath + "throttle.ini") && File.Exists(settingspath + "brake.ini") && File.Exists(settingspath + "clutch.ini") && File.Exists(settingspath + "handbrake.ini") && File.Exists(settingspath + "gearup.ini") && File.Exists(settingspath + "geardown.ini") && File.Exists(settingspath + "udp_ports.ini"))
                reading_settings();
            else Directory.CreateDirectory(settingspath);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 7)
            {
                this.Size = new System.Drawing.Size(790, 890);
                this.tabControl1.Size = new System.Drawing.Size(770, 780);
                try
                {
                    Settings_enable = true;
                    foreach (Joystick test in joysticks_list)
                    {
                        test.Acquire();
                    }
                }
                catch
                {
                    Settings_enable = false;
                    MessageBox.Show("Wrong settings\nCheck settings and try again");
                    foreach (Joystick test in joysticks_list)
                    {
                        test.Unacquire();
                    }
                }
                Left_foot_brakingAsync();
            }
            else
            {
                Settings_enable = false;
                this.Size = new System.Drawing.Size(1375, 513);
                this.tabControl1.Size = new System.Drawing.Size(1359, 410);
                if (tabControl1.SelectedIndex == 6)
                {
                    devicestable.Rows.Clear();
                    for (int i = 0; i < deviceslist.Count; i++)
                    {
                        devicestable.Rows.Add
                        (
                        deviceslist[i].InstanceName,
                        deviceslist[i].Type,
                        new Joystick(directinput, deviceslist[i].InstanceGuid).Capabilities.AxeCount,
                        new Joystick(directinput, deviceslist[i].InstanceGuid).Capabilities.ButtonCount,
                        isaxisavaliable(i, "X"),
                        isaxisavaliable(i, "Y"),
                        isaxisavaliable(i, "Z"),
                        isaxisavaliable(i, "RotationX"),
                        isaxisavaliable(i, "RotationY"),
                        isaxisavaliable(i, "RotationZ"),
                        isaxisavaliable(i, "Sliders0"),
                        isaxisavaliable(i, "Sliders1")
                            );
                    }
                    dataGridView1.DataSource = devicestable;
                    dataGridView1.AutoResizeColumns();
                }
            }
            
        }

        private void checkBox_clutch_is_axis_CheckedChanged(object sender, EventArgs e)
        {
            comboBox_axis_clutch.Items.Clear();
            if (checkBox_clutch_is_axis.CheckState == CheckState.Checked)
            {
                comboBox_axis_clutch.Items.AddRange(device_axis_list.ToArray());
            }
            if (checkBox_clutch_is_axis.CheckState == CheckState.Unchecked)
            {
                comboBox_axis_clutch.Items.AddRange(device_button_list.ToArray());
            }
        }

        private void checkBox_handbrake_is_axis_CheckedChanged(object sender, EventArgs e)
        {
            comboBox_axis_handbrake.Items.Clear();
            if (checkBox_handbrake_is_axis.CheckState == CheckState.Checked)
            {
                comboBox_axis_handbrake.Items.AddRange(device_axis_list.ToArray());
            }
            if (checkBox_handbrake_is_axis.CheckState == CheckState.Unchecked)
            {
                comboBox_axis_handbrake.Items.AddRange(device_button_list.ToArray());
            }
        }


        //Setting Section

        async void Settings_Section()
        {
            // update devices
            JoystickState[] js_array_test = new JoystickState[joysticks_list.Count];
            int throttle_value_nr = 0;
            int steer_value_nr = 0;
            int handbrale_value_nr = 0;
            int clutch_value_nr = 0;
            int brake_value_nr = 0;
            bool gearup_value = false;
            bool geardown_value = false;
            try
            {
                Settings_enable = true;
                foreach (Joystick test in joysticks_list)
                {
                    test.Acquire();
                }
                    steer_id_dv = devices_name.FindIndex(cb_device_steer.Text.Equals);
                    throttle_id_dv = devices_name.FindIndex(cb_device_throttle.Text.Equals);
                    brake_id_dv = devices_name.FindIndex(cb_device_brake.Text.Equals);
                    clutch_id_dv = devices_name.FindIndex(cb_device_clutch.Text.Equals);
                    handbrake_id_dv = devices_name.FindIndex(cb_device_handbrake.Text.Equals);
                    gearup_id_dv = devices_name.FindIndex(cb_device_gearup.Text.Equals);
                    geardown_id_dv = devices_name.FindIndex(cb_device_geardown.Text.Equals);

                for (int i = 0; i < joysticks_list.Count; i++)
                {
                    js_array_test[i] = joysticks_list[i].GetCurrentState();
                }

                steer_half = CheckAxis.GetAxisMax(comboBox_axis_steering.Text, steer_id_dv, joysticks_list) / 2;
                throttle_max = CheckAxis.GetAxisMax(comboBox_axis_throttle.Text, throttle_id_dv, joysticks_list);
                brake_max = CheckAxis.GetAxisMax(comboBox_axis_brake.Text, brake_id_dv, joysticks_list);
                clutch_max = CheckAxis.GetAxisMax(comboBox_axis_clutch.Text, clutch_id_dv, joysticks_list);
                handbrake_max = CheckAxis.GetAxisMax(comboBox_axis_handbrake.Text, handbrake_id_dv, joysticks_list);
            }
            catch
            {
                Settings_enable = false;
                MessageBox.Show("Wrong settings\nCheck settings and try again");
                foreach (Joystick test in joysticks_list)
                {
                    test.Unacquire();
                }
            }

            while (Settings_enable == true)
            {
                try
                {
                    label4.Invoke((MethodInvoker)(() => label4.Text = ""));
                    for (int i = 0; i < joysticks_list.Count; i++)
                    {
                        js_array_test[i] = joysticks_list[i].GetCurrentState();
                    }


                    //Steering Section

                    try 
                    {
                        steer_id_dv = devices_name.FindIndex(cb_device_steer.Text.Equals);
                        steer_half = CheckAxis.GetAxisMax(comboBox_axis_steering.Text, steer_id_dv, joysticks_list) / 2;
                        steer_value_nr = CheckAxis.SteerValue(comboBox_axis_steering.Text, steer_id_dv, steer_half, js_array_test);
                        trackBar_steering.Invoke((MethodInvoker)(() =>
                        {
                            if (checkBox_steer.Checked == true)
                                trackBar_steering.Value = steer_value_nr * -1;
                            else trackBar_steering.Value = steer_value_nr;
                        }));
                    }
                    catch
                    {
                        label4.Invoke((MethodInvoker)(() => label4.Text = "Wrong Steering"));
                    }

                    try
                    {
                        throttle_id_dv = devices_name.FindIndex(cb_device_throttle.Text.Equals);
                        throttle_max = CheckAxis.GetAxisMax(comboBox_axis_throttle.Text, throttle_id_dv, joysticks_list);
                        throttle_value_nr = CheckAxis.AxisValue(comboBox_axis_throttle.Text, throttle_id_dv, throttle_max, js_array_test);
                        checkBox_throttle.Invoke((MethodInvoker)(() =>
                        {
                            if (checkBox_throttle.Checked == true)
                                trackBar_throttle.Value = throttle_value_nr * -1 + 100;
                            else trackBar_throttle.Value = throttle_value_nr;
                        }));
                    }
                    catch
                    {
                        label4.Invoke((MethodInvoker)(() => label4.Text = "Wrong Throttle"));
                    }

                    try
                    {
                        brake_id_dv = devices_name.FindIndex(cb_device_brake.Text.Equals);
                        brake_max = CheckAxis.GetAxisMax(comboBox_axis_brake.Text, brake_id_dv, joysticks_list);
                        brake_value_nr = CheckAxis.AxisValue(comboBox_axis_brake.Text, brake_id_dv, brake_max, js_array_test);
                        trackBar_brake.Invoke((MethodInvoker)(() =>
                        {
                            if (checkBox_brake.Checked == true)
                                trackBar_brake.Value = brake_value_nr * -1 + 100;
                            else trackBar_brake.Value = brake_value_nr;
                        }));
                    }
                    catch
                    {
                        label4.Invoke((MethodInvoker)(() => label4.Text = "Wrong Brake"));
                    }

                    try
                    {
                        clutch_id_dv = devices_name.FindIndex(cb_device_clutch.Text.Equals);
                        clutch_max = CheckAxis.GetAxisMax(comboBox_axis_clutch.Text, clutch_id_dv, joysticks_list);
                        if (checkBox_clutch_is_axis.Checked == false)
                        {
                            clutch_value_nr = CheckAxis.ButtonValue(comboBox_axis_clutch.Text, clutch_id_dv, js_array_test);
                        }
                        if (checkBox_clutch_is_axis.Checked == true)
                        {
                            clutch_value_nr = CheckAxis.AxisValue(comboBox_axis_clutch.Text, clutch_id_dv, clutch_max, js_array_test);
                        }
                        trackBar_clutch.Invoke((MethodInvoker)(() =>
                        {
                            if (checkBox_clutch.Checked == true)
                                trackBar_clutch.Value = clutch_value_nr * -1 + 100;
                            else trackBar_clutch.Value = clutch_value_nr;
                        }));
                    }
                    catch
                    {
                        label4.Invoke((MethodInvoker)(() => label4.Text = "Wrong Clutch"));
                    }

                    try
                    {
                        handbrake_id_dv = devices_name.FindIndex(cb_device_handbrake.Text.Equals);
                        handbrake_max = CheckAxis.GetAxisMax(comboBox_axis_handbrake.Text, handbrake_id_dv, joysticks_list);
                        if (checkBox_handbrake_is_axis.Checked == false)
                        {
                            handbrale_value_nr = CheckAxis.ButtonValue(comboBox_axis_handbrake.Text, handbrake_id_dv, js_array_test);
                        }
                        if (checkBox_handbrake_is_axis.Checked == true)
                        {
                            handbrale_value_nr = CheckAxis.AxisValue(comboBox_axis_handbrake.Text, handbrake_id_dv, handbrake_max, js_array_test);
                        }
                        trackBar_Handbrake.Invoke((MethodInvoker)(() =>
                        {
                            if (checkBox_handbrake.Checked == true)
                                trackBar_Handbrake.Value = handbrale_value_nr * -1 + 100;
                            else trackBar_Handbrake.Value = handbrale_value_nr;
                        }));
                    }
                    catch
                    {
                        label4.Invoke((MethodInvoker)(() => label4.Text = "Wrong Handbrake"));
                    }

                    try
                    {
                        gearup_id_dv = devices_name.FindIndex(cb_device_gearup.Text.Equals);
                        gearup_value = js_array_test[gearup_id_dv].Buttons[Convert.ToInt16(comboBox_axis_gearup.Text.Split(' ')[1])];
                        if (gearup_value == true)
                            label4.Invoke((MethodInvoker)(() => label4.Text = "GearUP"));
                    }
                    catch
                    {
                        label4.Invoke((MethodInvoker)(() => label4.Text = "Wrong Gear UP"));
                    }

                    try
                    {
                        geardown_id_dv = devices_name.FindIndex(cb_device_geardown.Text.Equals);
                        geardown_value = js_array_test[geardown_id_dv].Buttons[Convert.ToInt16(comboBox_axis_geardown.Text.Split(' ')[1])];
                        if (geardown_value == true)
                            label4.Invoke((MethodInvoker)(() => label4.Text = "GearDOWN"));
                    }
                    catch
                    {
                        label4.Invoke((MethodInvoker)(() => label4.Text = "Wrong Gear DOWN"));
                    }
                }
                catch
                {
                    label4.Invoke((MethodInvoker)(() => label4.Text = "WORNG SETTINGS"));
                }
                await Task.Delay(100);
            }
            foreach (Joystick test in joysticks_list)
            {
                test.Unacquire();
            }
        }

        private void reading_settings()
        {
            using (StreamReader sr = new StreamReader(settingspath + "steering.ini"))
            {
                str_dv = sr.ReadLine();
                var srt_axis = sr.ReadLine();
                //if (str_dv != null)
                steer_id_dv = devices_id_str.FindIndex(str_dv.Equals);
                if (steer_id_dv == -1)
                {
                    cb_device_steer.Text = "Not Found";
                    steer_axis = comboBox_axis_steering.Text = "Not Found";
                }
                else
                {
                    cb_device_steer.Text = devices_name[steer_id_dv].ToString();
                    steer_axis = comboBox_axis_steering.Text = srt_axis;
                }
                var inverted = sr.ReadLine();
                if (inverted == "Checked")
                {
                    steer_inverted = true;
                    checkBox_steer.CheckState = CheckState.Checked;
                }
                else
                {
                    checkBox_steer.CheckState = CheckState.Unchecked;
                    steer_inverted = false;
                }
                float.TryParse(sr.ReadLine(), out steer_max);
            }
            using (StreamReader sr = new StreamReader(settingspath + "throttle.ini"))
            {
                thr_dv = sr.ReadLine();
                var thr_axis = sr.ReadLine();
                throttle_id_dv = devices_id_str.FindIndex(thr_dv.Equals);
                if (throttle_id_dv == -1)
                {
                    cb_device_throttle.Text = "Not Found";
                    throttle_axis = comboBox_axis_throttle.Text = "Not Found";
                }
                else
                {
                    cb_device_throttle.Text = devices_name[throttle_id_dv].ToString();
                    throttle_axis = comboBox_axis_throttle.Text = thr_axis;
                }

                var inverted = sr.ReadLine();
                if (inverted == "Checked")
                {
                    checkBox_throttle.CheckState = CheckState.Checked;
                    throttle_inverted = true;
                }
                else
                {
                    checkBox_throttle.CheckState = CheckState.Unchecked;
                    throttle_inverted = false;
                }
                float.TryParse(sr.ReadLine(), out throttle_max);
            }
            using (StreamReader sr = new StreamReader(settingspath + "brake.ini"))
            {
                brk_dv = sr.ReadLine();
                var brk_axis = sr.ReadLine();
                brake_id_dv = devices_id_str.FindIndex(brk_dv.Equals);
                if (brake_id_dv == -1)
                {
                    cb_device_brake.Text = "Not Found";
                    brake_axis = comboBox_axis_brake.Text = "Not Found";
                }
                else
                {
                    cb_device_brake.Text = devices_name[brake_id_dv].ToString();
                    brake_axis = comboBox_axis_brake.Text = brk_axis;
                }


                var inverted = sr.ReadLine();
                if (inverted == "Checked")
                {
                    checkBox_brake.CheckState = CheckState.Checked;
                    brake_inverted = true;
                }
                else
                {
                    checkBox_brake.CheckState = CheckState.Unchecked;
                    brake_inverted = false;
                }
                float.TryParse(sr.ReadLine(), out brake_max);
            }
            using (StreamReader sr = new StreamReader(settingspath + "clutch.ini"))
            {
                clt_dv = sr.ReadLine();
                clutch_id_dv = devices_id_str.FindIndex(clt_dv.Equals);
                var axis = sr.ReadLine();
                if (axis == "Checked")
                {
                    checkBox_clutch_is_axis.CheckState = CheckState.Checked;
                    clutch_isaxis = true;
                }
                else
                {
                    checkBox_clutch_is_axis.CheckState = CheckState.Unchecked;
                    clutch_isaxis = false;
                }
                clutch_axis  = sr.ReadLine();
                if (clutch_id_dv == -1)
                {
                    cb_device_clutch.Text = "Not Found";
                    clutch_axis = "Not Found";
                    comboBox_axis_clutch.Text = "Not Found";
                }
                else
                {
                    cb_device_clutch.Text = devices_name[clutch_id_dv].ToString();
                    comboBox_axis_clutch.Text = clutch_axis;
                }
                var inverted = sr.ReadLine();
                if (inverted == "Checked")
                {
                    checkBox_clutch.CheckState = CheckState.Checked;
                    clutch_inverted = true;
                }
                else
                {
                    checkBox_clutch.CheckState = CheckState.Unchecked;
                    clutch_inverted = false;
                }
                float.TryParse(sr.ReadLine(), out clutch_max);

            }
            using (StreamReader sr = new StreamReader(settingspath + "handbrake.ini"))
            {
                hbk_dv = sr.ReadLine();
                handbrake_id_dv = devices_id_str.FindIndex(hbk_dv.Equals);


                var axis = sr.ReadLine();
                if (axis == "Checked")
                {
                    checkBox_handbrake_is_axis.CheckState = CheckState.Checked;
                    handbrake_isaxis = true;
                }
                else
                {
                    checkBox_handbrake_is_axis.CheckState = CheckState.Unchecked;
                    handbrake_isaxis = false;
                }
                handbrake_axis = sr.ReadLine();
                if (handbrake_id_dv == -1)
                {
                    cb_device_handbrake.Text = "Not Found";
                    handbrake_axis = "Not Found";
                    comboBox_axis_handbrake.Text = "Not Found";
                }
                else
                {
                    cb_device_handbrake.Text = devices_name[handbrake_id_dv].ToString();
                    comboBox_axis_handbrake.Text = handbrake_axis;
                }
                var inverted = sr.ReadLine();
                if (inverted == "Checked")
                {
                    checkBox_handbrake.CheckState = CheckState.Checked;
                    handbrake_inverted = true;
                }
                else
                {
                    checkBox_handbrake.CheckState = CheckState.Unchecked;
                    handbrake_inverted = false;
                }
                float.TryParse(sr.ReadLine(), out handbrake_max);
            }
            using (StreamReader sr = new StreamReader(settingspath + "gearup.ini"))
            {
                gup_dv = sr.ReadLine();
                gearup_id_dv = devices_id_str.FindIndex(gup_dv.Equals);
                gearup_axis  = sr.ReadLine();
                if (gearup_id_dv == -1)
                {
                    cb_device_gearup.Text = "Not Found";
                    gearup_axis = "Not Found";
                    comboBox_axis_gearup.Text = "Not Found";
                }
                else
                {
                    cb_device_gearup.Text = devices_name[gearup_id_dv].ToString();
                    comboBox_axis_gearup.Text = gearup_axis;
                }


            }
            using (StreamReader sr = new StreamReader(settingspath + "geardown.ini"))
            {
                gdw_dv = sr.ReadLine();
                geardown_id_dv = devices_id_str.FindIndex(gdw_dv.Equals);
                geardown_axis  = sr.ReadLine();
                if (geardown_id_dv == -1)
                {
                    cb_device_geardown.Text = "Not Found";
                    geardown_axis = "Not Found";
                    comboBox_axis_geardown.Text = "Not Found";
                }
                else { cb_device_geardown.Text = devices_name[geardown_id_dv].ToString();
                    comboBox_axis_geardown.Text = geardown_axis ;
                }

            }
            using (StreamReader sr = new StreamReader(settingspath + "udp_ports.ini"))
            {
                int.TryParse(sr.ReadLine(), out lfs_udp_port);
                int.TryParse(sr.ReadLine(), out rbr_udp_port);
                int.TryParse(sr.ReadLine(), out dr_udp_port);
                int.TryParse(sr.ReadLine(), out fh_udp_port);
                lfs_udp_tb.Text = lfs_udp_port.ToString();
                rbr_udp_tb.Text = rbr_udp_port.ToString();
                dr_udp_tb.Text = dr_udp_port.ToString();
                fh_udp_tb.Text = fh_udp_port.ToString();
            }
            try
            {
                using (StreamReader sr = new StreamReader(settingspath + "export.ini"))
                {
                    float.TryParse(sr.ReadLine(), out mp);
                }
            }
            catch { }

        }

        public void CheckSettings()
        {

            //Steering Section
            if (steer_axis.Contains(" "))
            {
                var ts = steer_axis.Split(' ');
                steer_max = joysticks_list[steer_id_dv].GetObjectPropertiesByName(ts[0] + ts[1]).Range.Maximum;
            }
            else
            {
                steer_max = joysticks_list[steer_id_dv].GetObjectPropertiesByName(steer_axis).Range.Maximum;
            }
            //Throttle Section
            if (throttle_axis.Contains(" "))
            {
                var ts = throttle_axis.Split(' ');
                throttle_max = joysticks_list[throttle_id_dv].GetObjectPropertiesByName(ts[0] + ts[1]).Range.Maximum;
            }
            else
            {
                throttle_max = joysticks_list[throttle_id_dv].GetObjectPropertiesByName(throttle_axis).Range.Maximum;
            }
            //Brake Section
            if (brake_axis.Contains(" "))
            {
                var ts = brake_axis.Split(' ');
                brake_max = joysticks_list[brake_id_dv].GetObjectPropertiesByName(ts[0] + ts[1]).Range.Maximum;
            }
            else
            {
                brake_max = joysticks_list[brake_id_dv].GetObjectPropertiesByName(brake_axis).Range.Maximum;
            }
            // Clutch Section
            if (checkBox_clutch_is_axis.Checked == true)
            {
                if (clutch_axis.Contains(" "))
                {
                    var ts = clutch_axis.Split(' ');
                    clutch_max = joysticks_list[clutch_id_dv].GetObjectPropertiesByName(ts[0] + ts[1]).Range.Maximum;
                }
                else
                {
                    clutch_max = joysticks_list[clutch_id_dv].GetObjectPropertiesByName(clutch_axis).Range.Maximum;
                }
            }

            // Handbrake Section

            if (checkBox_handbrake_is_axis.Checked == true)
            {
                if (handbrake_axis.Contains(" "))
                {
                    var ts = handbrake_axis.Split(' ');
                    handbrake_max = joysticks_list[handbrake_id_dv].GetObjectPropertiesByName(ts[0] + ts[1]).Range.Maximum;
                }
                else
                {
                    handbrake_max = joysticks_list[handbrake_id_dv].GetObjectPropertiesByName(handbrake_axis).Range.Maximum;
                }
            }

        }

        public void GetDevicesData()
        {
            //joystickStates_list.Clear();
            foreach (Joystick js in joysticks_list)
            {
                //joystickStates_list.Add(js.GetCurrentState());
            }
            //joystickStates_list_all.Add(joystickStates_list);            
        }

        public string isaxisavaliable(int id, string an)
        {
            try
            {
                var isA = joysticks_list[id].GetObjectPropertiesByName(an).Range.Maximum;
                return "Y";
            }
            catch
            {
                return "N";
            }
        }

        private void button_savesettings_Click(object sender, EventArgs e)
        {
            Settings_enable = false;

            steer_axis = comboBox_axis_steering.Text;
            steer_inverted = Convert.ToBoolean(checkBox_steer.CheckState);
            steer_id_dv = devices_name.FindIndex(cb_device_steer.Text.Equals);
            try
            {
                var j = joysticks_list[steer_id_dv].GetObjectPropertiesByName(comboBox_axis_steering.Text).Range.Maximum;
            }
            catch
            {
                steer_id_dv = -1;
            }
            if (steer_id_dv !=-1)
            using (StreamWriter sw = new StreamWriter(settingspath + "steering.ini"))
            {
                //sw.WriteLine(String.Join("", cb_device_steer.Text));
                sw.WriteLine(String.Join("", devices_id[cb_device_steer.SelectedIndex].ToString()));
                sw.WriteLine(String.Join("", comboBox_axis_steering.Text));
                sw.WriteLine(String.Join("", checkBox_steer.CheckState));
                sw.WriteLine(String.Join("", joysticks_list[steer_id_dv].GetObjectPropertiesByName(comboBox_axis_steering.Text).Range.Maximum));

            }
            else
            {
                MessageBox.Show("Wrong steering settings");
            }

            
            throttle_axis = comboBox_axis_throttle.Text;
            throttle_inverted = Convert.ToBoolean(checkBox_throttle.CheckState);
            throttle_id_dv = devices_name.FindIndex(cb_device_throttle.Text.Equals);
            try
            {
                var j = joysticks_list[throttle_id_dv].GetObjectPropertiesByName(comboBox_axis_throttle.Text).Range.Maximum;
            }
            catch
            {
                throttle_id_dv = -1;
            }
            if (throttle_id_dv != -1)
                using (StreamWriter sw = new StreamWriter(settingspath + "throttle.ini"))
            {
                sw.WriteLine(String.Join("", devices_id[cb_device_throttle.SelectedIndex].ToString()));
                //sw.WriteLine(String.Join("", cb_device_throttle.Text));
                sw.WriteLine(String.Join("", comboBox_axis_throttle.Text));
                sw.WriteLine(String.Join("", checkBox_throttle.CheckState));
                sw.WriteLine(String.Join("", joysticks_list[throttle_id_dv].GetObjectPropertiesByName(comboBox_axis_throttle.Text).Range.Maximum));
            }
            else
            {
                MessageBox.Show("Wrong throttle settings");
            }

            brake_axis = comboBox_axis_brake.Text;
            brake_inverted = Convert.ToBoolean(checkBox_brake.CheckState);
            brake_id_dv = devices_name.FindIndex(cb_device_brake.Text.Equals);
            try
            {
                var j = joysticks_list[brake_id_dv].GetObjectPropertiesByName(comboBox_axis_brake.Text).Range.Maximum;
            }
            catch
            {
                brake_id_dv = -1;
            }
            if (brake_id_dv != -1)
                using (StreamWriter sw = new StreamWriter(settingspath + "brake.ini"))
            {
                sw.WriteLine(String.Join("", devices_id[cb_device_brake.SelectedIndex].ToString()));
                //sw.WriteLine(String.Join("", cb_device_brake.Text));
                sw.WriteLine(String.Join("", comboBox_axis_brake.Text));
                sw.WriteLine(String.Join("", checkBox_brake.CheckState));
                    sw.WriteLine(String.Join("", joysticks_list[brake_id_dv].GetObjectPropertiesByName(comboBox_axis_brake.Text).Range.Maximum));
            }
            else
            {
                MessageBox.Show("Wrong brake settings");
            }

            clutch_axis = comboBox_axis_clutch.Text;
            clutch_inverted = Convert.ToBoolean(checkBox_clutch.CheckState);
            clutch_isaxis = Convert.ToBoolean(checkBox_clutch_is_axis.CheckState);
            clutch_id_dv = devices_name.FindIndex(cb_device_clutch.Text.Equals);
            try
            {
                int j = 0;
                if (comboBox_axis_clutch.Text.Contains(" ") == true)
                {
                    if (checkBox_clutch_is_axis.Checked == true)
                        j = joysticks_list[clutch_id_dv].GetObjectPropertiesByName(comboBox_axis_clutch.Text.Split(' ')[0] + comboBox_axis_clutch.Text.Split(' ')[1]).Range.Maximum;
                    else;
                }
                else
                {
                    j = joysticks_list[clutch_id_dv].GetObjectPropertiesByName(comboBox_axis_clutch.Text).Range.Maximum;
                }
            }
            catch
            {
                clutch_id_dv = -1;
            }
            if (clutch_id_dv != -1)
                using (StreamWriter sw = new StreamWriter(settingspath + "clutch.ini"))
            {
                sw.WriteLine(String.Join("", devices_id[cb_device_clutch.SelectedIndex].ToString()));
                //sw.WriteLine(String.Join("", cb_device_clutch.Text));
                sw.WriteLine(String.Join("", checkBox_clutch_is_axis.CheckState));
                sw.WriteLine(String.Join("", comboBox_axis_clutch.Text));
                sw.WriteLine(String.Join("", checkBox_clutch.CheckState));
                    int j = 0;
                    if (comboBox_axis_clutch.Text.Contains(" ") == true)
                    {
                        if (checkBox_clutch_is_axis.Checked == true)
                            sw.WriteLine(String.Join("", joysticks_list[clutch_id_dv].GetObjectPropertiesByName(comboBox_axis_clutch.Text.Split(' ')[0] + comboBox_axis_clutch.Text.Split(' ')[1]).Range.Maximum));
                        else
                            sw.WriteLine(String.Join("", "-1"));
                    }
                    else
                    {
                        sw.WriteLine(String.Join("", joysticks_list[clutch_id_dv].GetObjectPropertiesByName(comboBox_axis_clutch.Text).Range.Maximum));
                    }
                }
            else
            {
                MessageBox.Show("Wrong clutch settings");
            }

            handbrake_axis = comboBox_axis_handbrake.Text;
            handbrake_inverted = Convert.ToBoolean(checkBox_handbrake.CheckState);
            handbrake_isaxis = Convert.ToBoolean(checkBox_handbrake_is_axis.CheckState);
            handbrake_id_dv = devices_name.FindIndex(cb_device_handbrake.Text.Equals);
            try
            {
                int j = 0;
                if (comboBox_axis_handbrake.Text.Contains(" ") == true)
                {
                    if (checkBox_handbrake_is_axis.Checked == true)
                        j = joysticks_list[handbrake_id_dv].GetObjectPropertiesByName(comboBox_axis_handbrake.Text.Split(' ')[0] + comboBox_axis_handbrake.Text.Split(' ')[1]).Range.Maximum;
                    else;
                }
                else
                {
                    j = joysticks_list[handbrake_id_dv].GetObjectPropertiesByName(comboBox_axis_handbrake.Text).Range.Maximum;
                }

            }
            catch
            {
                handbrake_id_dv = -1;
            }
            if (handbrake_id_dv != -1)
                using (StreamWriter sw = new StreamWriter(settingspath + "handbrake.ini"))
            {
                sw.WriteLine(String.Join("", devices_id[cb_device_handbrake.SelectedIndex].ToString()));
                //sw.WriteLine(String.Join("", cb_device_handbrake.Text));
                sw.WriteLine(String.Join("", checkBox_handbrake_is_axis.CheckState));
                sw.WriteLine(String.Join("", comboBox_axis_handbrake.Text));
                sw.WriteLine(String.Join("", checkBox_handbrake.CheckState));
                    int j = 0;
                    if (comboBox_axis_handbrake.Text.Contains(" ") == true)
                    {
                        if (checkBox_handbrake_is_axis.Checked == true)
                        sw.WriteLine(String.Join("", joysticks_list[handbrake_id_dv].GetObjectPropertiesByName(comboBox_axis_clutch.Text.Split(' ')[0] + comboBox_axis_clutch.Text.Split(' ')[1]).Range.Maximum));
                        else 
                        sw.WriteLine(String.Join("", "-1"));
                    }
                    else
                    {
                        sw.WriteLine(String.Join("", joysticks_list[handbrake_id_dv].GetObjectPropertiesByName(comboBox_axis_handbrake.Text).Range.Maximum));
                    }


                    //if (handbrake_isaxis == true)
                    //    sw.WriteLine(String.Join("", joysticks_list[handbrake_id_dv].GetObjectPropertiesByName(comboBox_axis_handbrake.Text).Range.Maximum));
                    //else sw.WriteLine(String.Join("", "-1"));

            }
            else
            {
                MessageBox.Show("Wrong handbrake settings");
            }

            gearup_axis = comboBox_axis_gearup.Text;
            gearup_id_dv = devices_name.FindIndex(cb_device_gearup.Text.Equals); 
            if (gearup_id_dv != -1)
                using (StreamWriter sw = new StreamWriter(settingspath + "gearup.ini"))
            {
                sw.WriteLine(String.Join("", devices_id[cb_device_gearup.SelectedIndex].ToString()));
                //sw.WriteLine(String.Join("", cb_device_gearup.Text));
                sw.WriteLine(String.Join("", comboBox_axis_gearup.Text));
            }
            else
            {
                MessageBox.Show("Wrong gear up settings");
            }

            
            
            geardown_axis = comboBox_axis_geardown.Text;
            geardown_id_dv = devices_name.FindIndex(cb_device_geardown.Text.Equals);
            if (geardown_id_dv != -1)
                using (StreamWriter sw = new StreamWriter(settingspath + "geardown.ini"))
            {
                sw.WriteLine(String.Join("", devices_id[cb_device_geardown.SelectedIndex].ToString()));
                //sw.WriteLine(String.Join("", cb_device_geardown.Text));
                sw.WriteLine(String.Join("", comboBox_axis_geardown.Text));
            }
            else
            {
                MessageBox.Show("Wrong gear down settings");
            }

            lfs_udp_port = Convert.ToInt32(lfs_udp_tb.Text);
            rbr_udp_port = Convert.ToInt32(rbr_udp_tb.Text);
            dr_udp_port = Convert.ToInt32(dr_udp_tb.Text);
            fh_udp_port = Convert.ToInt32(fh_udp_tb.Text);
            using (StreamWriter sw = new StreamWriter(settingspath + "udp_ports.ini"))
            {
                sw.WriteLine(String.Join("", lfs_udp_tb.Text));
                sw.WriteLine(String.Join("", rbr_udp_tb.Text));
                sw.WriteLine(String.Join("", dr_udp_tb.Text));
                sw.WriteLine(String.Join("", fh_udp_tb.Text));
                int.TryParse(lfs_udp_tb.Text, out lfs_udp_port);
                int.TryParse(rbr_udp_tb.Text, out rbr_udp_port);
                int.TryParse(dr_udp_tb.Text , out dr_udp_port);
                int.TryParse(fh_udp_tb.Text, out fh_udp_port);
            }

            using (StreamWriter sw = new StreamWriter(settingspath + "export.ini"))
            {
                sw.WriteLine(String.Join("", export_mp.Text));
            }

            MessageBox.Show("Settings saved");
            label4.Invoke((MethodInvoker)(() => label4.Text = ""));
        }

        public float avr_brk_pdl(float s0)
        {
            a_b_p[4] = a_b_p[3];
            a_b_p[3]=a_b_p[2];
            a_b_p[2]=a_b_p[1];
            a_b_p[1]=a_b_p[0];
            a_b_p[0] = s0;
            return a_b_p.Average();
           
        }
    }
}