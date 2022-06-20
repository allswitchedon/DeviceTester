using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp2
{
    public class HzCalc
    {
        public  static string GetHz(List<float> time)
        {
            List<int> int_time = new List<int>();
            List<int> hz_frq = new List<int>();
            string hz_frq_return = "Unknown ";
            for (int i = 0; i < time.Count; i++)
            {
                int_time.Add((int)time[i]);
            }
            for (int i = 1; i < int_time.Max(); i++)
            {
                hz_frq.Add(int_time.FindAll(x => x == i).Count);
            }
            try
            {
                hz_frq_return = hz_frq.Average().ToString();
            }
            catch
            {
            }
            return hz_frq_return;
        }

    }
}
