using System.Threading;
using System.Threading.Tasks;
using XenaxControl.Hardware.Drivers;

namespace XenaxControl.Hardware.Controllers
{
    public interface IAxisController
    {
        bool IsInMotion();

        void Initialize();

        Task InitializeAsync(CancellationToken token);

        void Move(Direction direction, double speedPercentage = 100);

        void MoveAbs(double millimetre, double speedPercentage = 100);

        Task MoveAbsAsync(double millimetre, CancellationToken token);

        Task MoveAbsAsync(double millimetre, double speedPercentage, CancellationToken token);

        void MoveRel(double millimetre, double speedPercentage = 100);

        Task MoveRelAsync(double millimetre, CancellationToken token);

        Task MoveRelAsync(double millimetre, double speedPercentage, CancellationToken token);

        void MoveOrigin(double speedPercentage = 100);

        Task MoveOriginAsync(CancellationToken token);

        Task MoveOriginAsync(double speedPercentage, CancellationToken token);

        void Stop(StopMode stopMode = StopMode.Immediate);

        Task StopAsync(StopMode stopMode, CancellationToken token);

        double GetPosition();

        int GetAbsPosition();

        double PulseToMillimetre(int pulse);

        int MillimetreToPulse(double millimetre);

        double ConvertToAdjustedMillimetre(int pulse);

        int ConvertToAdjustedPulse(double millimetre);
    }
}
