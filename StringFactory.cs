using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysMonitor
{
    class StringFactory
    {
        public StringFactory () {}


        public string loadString (float? value)
        {
            if (value != null)
            {
                decimal newValue = (decimal)value;
                newValue = Math.Ceiling(newValue);
                string outputString = $"Load: {newValue}%";
                return outputString;
            } else
            {
                return "Load: N/A";
            }
        }

        public string temperatureString(float? value)
        {
            if (value != null)
            {
                decimal newValue = (decimal)value;
                newValue = Math.Ceiling(newValue);
                string outputString = $"Temperature: {newValue}°C";
                return outputString;
            }
            else
            {
                return "Temperature: N/A";
            }
        }

        public string clockString (float? value)
        {
            if (value != null)
            {
                decimal newValue = (decimal)value;
                newValue = Math.Ceiling(newValue);
                string outputString = $"Clock: {newValue}MHz";
                return outputString;
            } else
            {
                return "Clock: N/A";
            }
        }

        public string memoryString(float? value)
        {
            if (value != null)
            {
                string newValue = MBToGB(value);
                string outputString = $"{newValue}";
                return outputString;
            }
            else
            {
                return "Memory: N/A";
            }
        }

        public string MBToGB (float? memory)
        {
            double memGB = (double)memory / 1024;

            return GBString ((float)memGB);
        }

        public int bytesToGB (ulong memory)
        {
            double memGB = memory / Math.Pow(1024, 3);
            memGB = Math.Round(memGB, 0, MidpointRounding.AwayFromZero);
            return (int)memGB;
        }

        public string GBString (float? memory)
        {
            double memGB = Math.Round((double)memory, 1, MidpointRounding.AwayFromZero);
            string outputString = $"{memGB}GB";
            return outputString;
        }
    }
}
