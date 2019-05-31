using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysMonitor
{
    public class Fan
    {
        private string fanType;
        public string FanType {
            get { return fanType; }
            set { fanType = value; }
        }

        private int rpm = 0;

        private int previousValue = 0;
        private int speedClassChangeCount = 0;
        private int speedClass = 0;

        public int SpeedClass
        {
            get { return speedClass; }
            set { speedClass = value; }
        }

        private int needsUpdate = -1;
        public int NeedsUpdate
        {
            get { return needsUpdate; }
            set { needsUpdate = value; }
        }

        public Fan (string fanType, int rpm)
        {
            this.fanType = fanType;
            if (fanType == "GPU")
                speedClass = 6;
            setRPM(rpm);
        }

        public int getRPM ()
        {
            return rpm;
        }

        //Sets speedClass to either -1 (No change to fan gif) or a number 0..5 to classify which fan gif should be set, 
        //also sets the needsUpdate flag
        public void setRPM (int newRPM)
        {
            previousValue = rpm;
            int newSpeedClass = checkSpeedClass(newRPM);
            if (newRPM != previousValue)
            {
                rpm = newRPM;

                if (speedClass != newSpeedClass)
                {
                    if (speedClassChangeCount < 5)
                    {
                        speedClassChangeCount++;
                        NeedsUpdate = -1;
                    }
                    else
                    {
                        speedClassChangeCount = 0;
                        speedClass = newSpeedClass;
                        NeedsUpdate = 0;
                    }
                } else
                {
                    needsUpdate = -1;
                }
            } else
            {
                needsUpdate = -1;
            }
        }

        private int checkSpeedClass(int rpm)
        {
            if (fanType != "GPU")
            {
                if (rpm > 0 && rpm <= 250)
                {
                    return 0;
                }
                else if (rpm > 250 && rpm <= 500)
                {
                    return 1;
                }
                else if (rpm > 500 && rpm <= 800)
                {
                    return 2;
                }
                else if (rpm > 800 && rpm <= 1150)
                {
                    return 3;
                }
                else if (rpm > 1150 && rpm <= 1500)
                {
                    return 4;
                }
                else if (rpm > 1500)
                {
                    return 5;
                }
                else
                {
                    return -1;
                }
            } else
            {
                if (rpm > 0 && rpm <= 500)
                {
                    return 6;
                } else if (rpm > 500 && rpm <= 1250)
                {
                    return 7;
                } else if (rpm > 1250)
                {
                    return 8;
                } else
                {
                    return -1;
                }
            }            
        }
    }
}
