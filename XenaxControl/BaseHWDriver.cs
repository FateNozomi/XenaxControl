using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenaxControl
{
    public abstract class BaseHWDriver
    {
        public virtual bool Connected { get; protected set; }

        public virtual bool Connect()
        {
            this.Connected = true;
            return true;
        }

        public virtual bool Disconnect()
        {
            this.Connected = false;
            return true;
        }
    }
}
