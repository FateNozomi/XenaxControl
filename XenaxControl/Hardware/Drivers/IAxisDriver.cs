namespace XenaxControl.Hardware.Drivers
{
    public interface IAxisDriver
    {
        bool IsInitialized(int axisId);

        bool IsInMotion(int axisId);

        void Initialize(int axisId, Direction direction);

        void Move(int axisId, Direction direction);

        void MoveAbs(int axisId, int absPosition);

        void MoveRel(int axisId, int relativeDistance);

        void Stop(int axisId, StopMode mode);

        void SetMovementParameter(
            int axisId,
            int initialSpeed,
            int finalSpeed,
            int accelerationDuration,
            int decelerationDuration,
            int scurveAccelerationPercentage,
            int scurveDecelerationPercentage);

        void SetMovementParameter(int axisId, int speed, int acceleration, int scurvePercentage);

        void SetSoftwareLimit(int axisId, int negativeLimit, int positiveLimit);

        void SetAbsPosition(int axisId, int position);

        int GetAbsPosition(int axisId);

        int GetAxisStatus(int axisId);

        void SetAlarmLogicLevel(int axisId, bool active);

        void SetServoState(int axisId, bool state);
    }
}
