using System.Threading;
using System.Threading.Tasks;

namespace XenaxControl.Hardware.Drivers
{
    public abstract class Driver
    {
        public string Type { get; protected set; }

        public string Version { get; protected set; }

        public string Id { get; protected set; }

        public virtual bool Connected { get; protected set; }

        public abstract bool Connect();

        public abstract Task<bool> ConnectAsync(CancellationToken token);

        public abstract bool Disconnect();
    }
}
