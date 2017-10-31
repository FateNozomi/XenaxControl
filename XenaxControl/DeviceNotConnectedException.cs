using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenaxControl
{
    public class DeviceNotConnectedException : Exception
    {
        public DeviceNotConnectedException()
        {
        }

        public DeviceNotConnectedException(string message)
            : base(message)
        {
        }

        public DeviceNotConnectedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
