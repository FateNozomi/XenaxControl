using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenaxControl
{
    public class HWDeviceManager : BaseHWDriver
    {
        public HWDeviceManager()
        {
            this.XenaxHWDrivers = new SortedDictionary<string, XenaxHWDriver>();
            this.XenaxHWControllers = new SortedDictionary<string, XenaxHWController>();
        }

        public SortedDictionary<string, XenaxHWDriver> XenaxHWDrivers { get; set; }

        public SortedDictionary<string, XenaxHWController> XenaxHWControllers { get; set; }
    }
}
