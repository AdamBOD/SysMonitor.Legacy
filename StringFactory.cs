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
    }
}
