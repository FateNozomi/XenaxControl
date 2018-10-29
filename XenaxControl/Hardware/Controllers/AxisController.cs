using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using XenaxControl.Hardware.Drivers;

namespace XenaxControl.Hardware.Controllers
{
    public abstract class AxisController : Controller, IAxisController
    {
        public IAxisDriver AxisDriver { get; private set; }

        public int AxisId { get; set; }

        public double DisplacementPerRev { get; set; }

        public int PulsePerRev { get; set; }

        public Direction Direction { get; set; }

        public double AbsoluteMillimetre { get; set; }

        public int NegativeLimit { get; set; }

        public int PositiveLimit { get; set; }

        public int InitialSpeed { get; set; }

        public int FinalSpeed { get; set; }

        public int AccelerationDuration { get; set; }

        public int DecelerationDuration { get; set; }

        public int Acceleration { get; set; }

        public int SCurvePercentage { get; set; }

        public int SCurveDecelerationPercentage { get; set; }

        public int Timeout { get; set; }

        public Direction InitializeDirection { get; set; }

        public double InitializeSpeedPercentage { get; set; }

        public int OriginPulse { get; set; }

        public bool ServoState { get; set; }

        public bool ServoAlarmLogicLevel { get; set; }

        public void LoadDriver(IAxisDriver axisDriver)
        {
            AxisDriver = axisDriver;
            DriverId = ((Driver)AxisDriver).Id;
        }

        public override void LoadSettings()
        {
        }

        public override void SaveSettings()
        {
        }

        public bool IsInMotion()
        {
            return AxisDriver.IsInMotion(AxisId);
        }

        public abstract void Initialize();

        public abstract Task InitializeAsync(CancellationToken token);

        public abstract void Move(Direction direction, double speedPercentage = 100);

        public void MoveAbs(double millimetre, double speedPercentage = 100)
        {
            SetParameter(speedPercentage);
            AxisDriver.SetSoftwareLimit(AxisId, NegativeLimit, PositiveLimit);
            int adjustedPulse = ConvertToAdjustedPulse(millimetre);
            AxisDriver.MoveAbs(AxisId, adjustedPulse);
        }

        public Task MoveAbsAsync(double millimetre, CancellationToken token)
        {
            return MoveAbsAsync(millimetre, 100, token);
        }

        public async Task MoveAbsAsync(double millimetre, double speedPercentage, CancellationToken token)
        {
            MoveAbs(millimetre, speedPercentage);
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                while (IsInMotion())
                {
                    if (sw.ElapsedMilliseconds > Timeout)
                    {
                        throw new TimeoutException($"{Id} axis motion has timed-out.");
                    }

                    await Task.Delay(50, token).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                Stop();
                throw;
            }
        }

        public void MoveRel(double millimetre, double speedPercentage = 100)
        {
            SetParameter(speedPercentage);
            AxisDriver.SetSoftwareLimit(AxisId, NegativeLimit, PositiveLimit);
            int adjustedPulse = MillimetreToPulse(millimetre);
            AxisDriver.MoveRel(AxisId, adjustedPulse * (sbyte)Direction);
        }

        public Task MoveRelAsync(double millimetre, CancellationToken token)
        {
            return MoveRelAsync(millimetre, 100, token);
        }

        public async Task MoveRelAsync(double millimetre, double speedPercentage, CancellationToken token)
        {
            MoveRel(millimetre, speedPercentage);
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                while (IsInMotion())
                {
                    if (sw.ElapsedMilliseconds > Timeout)
                    {
                        throw new TimeoutException($"{Id} axis motion has timed-out.");
                    }

                    await Task.Delay(50, token).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                Stop();
                throw;
            }
        }

        public void MoveOrigin(double speedPercentage = 100)
        {
            SetParameter(speedPercentage);
            AxisDriver.SetSoftwareLimit(AxisId, NegativeLimit, PositiveLimit);
            AxisDriver.MoveAbs(AxisId, OriginPulse);
        }

        public Task MoveOriginAsync(CancellationToken token)
        {
            return MoveOriginAsync(100, token);
        }

        public async Task MoveOriginAsync(double speedPercentage, CancellationToken token)
        {
            MoveOrigin(speedPercentage);
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                while (IsInMotion())
                {
                    if (sw.ElapsedMilliseconds > Timeout)
                    {
                        throw new TimeoutException($"{Id} axis motion has timed-out.");
                    }

                    await Task.Delay(50, token).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                Stop();
                throw;
            }
        }

        public void Stop(StopMode stopMode = StopMode.Immediate)
        {
            AxisDriver.Stop(AxisId, stopMode);
        }

        public async Task StopAsync(StopMode stopMode, CancellationToken token)
        {
            Stop(stopMode);
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                while (IsInMotion())
                {
                    if (sw.ElapsedMilliseconds > Timeout)
                    {
                        throw new TimeoutException($"{Id} axis motion has timed-out.");
                    }

                    await Task.Delay(50, token).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                Stop();
                throw;
            }
        }

        public double GetPosition()
        {
            int pulse = GetAbsPosition();
            return ConvertToAdjustedMillimetre(pulse);
        }

        public int GetAbsPosition()
        {
            return AxisDriver.GetAbsPosition(AxisId);
        }

        public double PulseToMillimetre(int pulse)
        {
            double millimetrePerPulse = DisplacementPerRev / PulsePerRev;

            double millimetre = pulse * millimetrePerPulse;

            return millimetre;
        }

        public int MillimetreToPulse(double millimetre)
        {
            double pulsePerMillimetre = PulsePerRev / DisplacementPerRev;

            double pulse = millimetre * pulsePerMillimetre;

            return (int)pulse;
        }

        public double ConvertToAdjustedMillimetre(int pulse)
        {
            double millimetre = PulseToMillimetre(pulse);

            return (millimetre / (sbyte)Direction) + AbsoluteMillimetre;
        }

        public int ConvertToAdjustedPulse(double millimetre)
        {
            double adjustedMillimetre = (millimetre - AbsoluteMillimetre) * (sbyte)Direction;

            return MillimetreToPulse(adjustedMillimetre);
        }

        protected void SetParameter(double speedPercentage = 100)
        {
            double adjustedSpeed = FinalSpeed * (speedPercentage / 100d);

            AxisDriver.SetMovementParameter(
                AxisId,
                (int)adjustedSpeed,
                Acceleration,
                SCurvePercentage);

            AxisDriver.SetMovementParameter(
                AxisId,
                InitialSpeed,
                (int)adjustedSpeed,
                AccelerationDuration,
                DecelerationDuration,
                SCurvePercentage,
                SCurveDecelerationPercentage);
        }
    }
}
