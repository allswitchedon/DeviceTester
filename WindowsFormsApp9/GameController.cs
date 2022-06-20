using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectInput;

namespace WinFormsApp2
{
    internal class GameController
    {
        //DirectInput directinput;
        static Joystick joystick;
        static JoystickState joystickstate;
        //static IList<DeviceInstance> deviceslist;
        static float throttle, brake, clutch, handbrake, steering, gearup, geardown;
        


        //static public IList<DeviceInstance> GetDevices()
        //{
        //    var deviceslist = directinput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
        //    return deviceslist;
        //}


        //public GameController()
        //{
        //    var list = directinput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
        //    joystick = new Joystick(directinput, list[0].InstanceGuid);
        //    joystick.Acquire();
        //}

        public GameController(DirectInput directinput, Guid instaceguid)
        {
            joystick = new Joystick(directinput, instaceguid);
            joystick.Acquire();
        }


        public static List<float> TrustmasterT300()
        {
            List<float> wheelbase = new List<float>();
            joystickstate = joystick.GetCurrentState();
            steering = (float)(joystickstate.X - (65535/2) )/ (65535 / 2);
            if (steering > 1)
                steering = 1;
            throttle = (float)joystickstate.RotationZ/ 65535 * -1 + 1;
            brake = (float)joystickstate.Y / 65535 * -1 + 1;
            clutch = (float)joystickstate.Sliders[0] / 65535 * -1 + 1;
            if (joystickstate.Buttons[9] == true)
                handbrake = 1;
            else handbrake = 0;

            if (joystickstate.Buttons[0] == true)
                geardown = 1;
            else geardown = 0;

            if (joystickstate.Buttons[1] == true)
                gearup = 1;
            else gearup = 0;
            //string values = "Steering: " + steering.ToString() + "\n" + "Throttle: " + throttle.ToString() + "\n" + "Brake: " + brake.ToString() + "\n" + "Clutch: " + clutch.ToString() + "\nHandbrake: " + handbrake.ToString() + "\nGearUp: " + gearup.ToString() + "\nGearDown: " + geardown.ToString();
            //string values = joystickstate.ToString();
            wheelbase.Add(steering);
            wheelbase.Add(throttle);
            wheelbase.Add(brake);                
            wheelbase.Add(clutch);
            wheelbase.Add(handbrake);
            wheelbase.Add(gearup);
            wheelbase.Add(geardown);



            return wheelbase;

        }
    }
}
