using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenaxControl
{
    public interface IDriver
    {
        int DriverCount { get; }

        bool Connected { get; }

        bool Connect();

        bool Disconnect();
    }
}
