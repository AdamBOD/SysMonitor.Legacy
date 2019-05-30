using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysMonitor
{
    class Fan
    {
        private string _type;
        public string type
        {
            get { return _type; }
            set { _type = value; }
        }

        private int _rpm;
        public int rpm
        {
            get { return _rpm; }
            set {
                previousValue = _rpm;
                if (value >= previousValue)
                {
                    if (rpmClassChangeCount < 5)
                    {
                        rpmClassChangeCount++;
                    }
                }
            }
        }

        private int previousValue = 0;
        private int rpmClassChangeCount = 0;
    }
}
