using XenaxControl.Hardware.Drivers;

namespace XenaxControl.Hardware.Controllers
{
    public abstract class Controller
    {
        public string Id { get; protected set; }

        public string DriverId { get; protected set; }

        public abstract void LoadSettings();

        public abstract void SaveSettings();
    }
}
