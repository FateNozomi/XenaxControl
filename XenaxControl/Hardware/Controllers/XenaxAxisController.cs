using System;
using System.Threading;
using System.Threading.Tasks;
using XenaxControl.Hardware.Drivers;

namespace XenaxControl.Hardware.Controllers
{
    public class XenaxAxisController : AxisController
    {
        public XenaxAxisController(string id)
        {
            Id = id;
            LoadDefaultParameters();
        }

        public override void Initialize()
        {
            AxisDriver.SetSoftwareLimit(AxisId, negativeLimit: 0, positiveLimit: 0);
            AxisDriver.Initialize(AxisId, InitializeDirection);
        }

        public override async Task InitializeAsync(CancellationToken token)
        {
            Initialize();

            try
            {
                while (!AxisDriver.IsInitialized(AxisId))
                {
                    await Task.Delay(10, token).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                Stop();
                throw;
            }
        }

        public override void Move(Direction direction, double speedPercentage = 100)
        {
            SetParameter(speedPercentage);
            AxisDriver.SetSoftwareLimit(AxisId, NegativeLimit, PositiveLimit);
            sbyte finalDirection = (sbyte)((sbyte)direction * (sbyte)Direction);
            AxisDriver.Move(AxisId, (Direction)finalDirection);
        }

        private void LoadDefaultParameters()
        {
            DisplacementPerRev = 1;
            PulsePerRev = 1000;
            NegativeLimit = 0;
            PositiveLimit = 800000;
            FinalSpeed = 100000;
            Acceleration = 1000000;
            SCurvePercentage = 100;
            Timeout = 15000;
        }
    }
}
