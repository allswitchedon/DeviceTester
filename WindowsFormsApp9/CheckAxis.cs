using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp2
{
    public class CheckAxis
    {
        public static int SteerValue(string str, int id_dv, float steer_half, SharpDX.DirectInput.JoystickState[] js_array_test)
        {
            int value = 0;
            if (str.Contains(" "))
            {
                value = (int)(float)(((Convert.ToSingle(js_array_test[id_dv].Sliders[Convert.ToInt16(str.Split(' ')[1])]) - steer_half) / steer_half) * 100);
            }
            else
            {
                value = (int)(float)(((Convert.ToSingle(js_array_test[id_dv].GetType().GetProperty(str).GetValue(js_array_test[id_dv])) - steer_half) / steer_half) * 100);
            }
            return value;
        }

        public static float SteerValue_f(string str, int id_dv, float steer_half, SharpDX.DirectInput.JoystickState[] js_array_test)
        {
            float value = 0;
            steer_half = 32767.5f;
            if (str.Contains(" "))
            {
                value = (float)(((Convert.ToSingle(js_array_test[id_dv].Sliders[Convert.ToInt16(str.Split(' ')[1])]) - steer_half) / steer_half));
            }
            else
            {
                value = (float)(((Convert.ToSingle(js_array_test[id_dv].GetType().GetProperty(str).GetValue(js_array_test[id_dv])) - steer_half) / steer_half));
            }
            return value;
        }

        public static float AxisValue_f(string str, int id, float max, SharpDX.DirectInput.JoystickState[] ja)
        {
            float value = 0;
            if (str.Contains(" "))
            {
                value = (float)(((Convert.ToSingle(ja[id].Sliders[Convert.ToInt16(str.Split(' ')[1])])) / max));
            }
            else
            {
                value =(float)(((Convert.ToSingle(ja[id].GetType().GetProperty(str).GetValue(ja[id]))) / max) );
            }
            return value;
        }




        public static int AxisValue(string str, int id, float max, SharpDX.DirectInput.JoystickState[] ja)
        {
            int value = 0;
            if (str.Contains(" "))
            {
                value = (int)(float)(((Convert.ToSingle(ja[id].Sliders[Convert.ToInt16(str.Split(' ')[1])])) / max) * 100);
            }
            else
            {
                value = (int)(float)(((Convert.ToSingle(ja[id].GetType().GetProperty(str).GetValue(ja[id]))) / max) * 100);
            }
            return value;
        }

        public static int ButtonValue(string str, int id, SharpDX.DirectInput.JoystickState[] ja)
        {
            int value = 0;
            value = Convert.ToInt32(ja[id].Buttons[Convert.ToInt16(str.Split(' ')[1])]) * 100;
            return value;
        }

        public static int GetAxisMax(string str, int id_dv, List<SharpDX.DirectInput.Joystick> jl)
        {
            int max = 0;
                if (str.Contains(" "))
                {
                    if (str.Contains("utton"))
                    {
                        max = 0;
                    }
                    else
                    {
                        var ts = str.Split(' ');
                        max = jl[id_dv].GetObjectPropertiesByName(ts[0] + ts[1]).Range.Maximum;
                    }
                }
                else
                {
                    max = jl[id_dv].GetObjectPropertiesByName(str).Range.Maximum;
                }
            return max;
        }
    }
}
