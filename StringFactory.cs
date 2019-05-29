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
            decimal newValue = (decimal)value;
            newValue = Math.Ceiling(newValue);
            string outputString = $"Load: {newValue}%";
            return outputString;
        }

        public string temperatureString(float? value)
        {
            decimal newValue = (decimal)value;
            newValue = Math.Ceiling(newValue);
            string outputString = $"Temperature: {newValue}°C";
            return outputString;
        }

        public string clockString (float? value)
        {
            decimal newValue = (decimal)value;
            newValue = Math.Ceiling(newValue);
            string outputString = $"Clock: {newValue}MHz";
            return outputString;
        }
    }
}
