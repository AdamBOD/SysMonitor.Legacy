using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysMonitor
{
    public class Helpers
    {
        public Helpers () { }

        // Function to return the heat class of the CPU or GPU
        public static int getProcessorHeatClass (float? temperature)
        {
            if (temperature < 45)
            {
                return 0;
            }
            else if (temperature >= 45 && temperature < 75)
            {
                return 1;
            }
            else if (temperature > 75)
            {
                return 2;
            }
            else
            {
                return -1;
            }
        }

        public static string getProcessorTemperatureIconPath (int heatClass)
        {
            string iconPath = "Resources\\Images\\CPU Status\\";
            switch (heatClass)
            {
                case 0:
                    iconPath += "CPUCool.png";
                    break;
                case 1:
                    iconPath += "CPUHot.png";
                    break;
                case 2:
                    iconPath += "CPUOverheated.png";
                    break;
                default:
                    break;
            }
            return iconPath;
        }

        // Function to build the URL for the Fan GIF based on the Fan type and speed
        public static string createGifPath(string GPUString, string fanType, int speedClass)
        {
            string gifPath = "Resources/GIFs/";
            if (fanType == "CPU")
            {
                gifPath += "CPU_Fans/";
                switch (speedClass)
                {
                    case 0:
                        gifPath += "CPU_Fan_Slow.gif";
                        break;
                    case 1:
                        gifPath += "CPU_Fan_Slow_Medium.gif";
                        break;
                    case 2:
                        gifPath += "CPU_Fan_Medium.gif";
                        break;
                    case 3:
                        gifPath += "CPU_Fan_Medium_Fast.gif";
                        break;
                    case 4:
                        gifPath += "CPU_Fan_Fast.gif";
                        break;
                    case 5:
                        gifPath += "CPU_Fan_Very_Fast.gif";
                        break;
                    default:
                        break;
                }
            }
            else if (fanType == "GPU")
            {
                gifPath += "GPUs/";
                switch (speedClass)
                {
                    case 6:
                        gifPath += $"{GPUString}-Slow.gif";
                        break;
                    case 7:
                        gifPath += $"{GPUString}-Medium.gif";
                        break;
                    case 8:
                        gifPath += $"{GPUString}-Fast.gif";
                        break;
                    default:
                        break;
                }
            }
            else if (fanType == "Case")
            {
                gifPath += "Fans/";
                switch (speedClass)
                {
                    case 0:
                        gifPath += "Fan_Slow.gif";
                        break;
                    case 1:
                        gifPath += "Fan_Slow_Medium.gif";
                        break;
                    case 2:
                        gifPath += "Fan_Medium.gif";
                        break;
                    case 3:
                        gifPath += "Fan_Medium_Fast.gif";
                        break;
                    case 4:
                        gifPath += "Fan_Fast.gif";
                        break;
                    case 5:
                        gifPath += "Fan_Very_Fast.gif";
                        break;
                    default:
                        break;
                }
            }
            return gifPath;
        }
    }
}
