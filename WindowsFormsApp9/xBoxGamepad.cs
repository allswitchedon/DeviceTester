using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.XInput;

namespace WinFormsApp2
{
    internal class xBoxGamepad
    {
        Controller controller;
        Gamepad gamepad;
        bool connected = false;
        int deadband = 2500;
        float leftTrigger, rightTrigger, leftThumb;

        public xBoxGamepad()
        {
            controller = new Controller(UserIndex.One);
            connected = controller.IsConnected;
        }

        // Call this method to update all class values
        public string Update()
        {
            if (!connected)
            {

                return "not connected";
            }
            gamepad = controller.GetState().Gamepad;
            leftThumb = (int)((Math.Abs((float)gamepad.LeftThumbX) < deadband) ? 0 : (float)gamepad.LeftThumbX / short.MaxValue * 100);
            leftTrigger = (int)(gamepad.LeftTrigger * 100) / 255;
            rightTrigger = (int)(gamepad.RightTrigger * 100) / 255;
            string trigers = "Throttle: " + rightTrigger.ToString() + "\n" + "Brake: " + leftTrigger.ToString() + "\n" + "Steering: " + leftThumb.ToString();

            return trigers;
        }
    }
}
