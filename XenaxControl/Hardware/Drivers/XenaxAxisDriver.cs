using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using XenaxControl.Utilities;

namespace XenaxControl.Hardware.Drivers
{
    public class XenaxAxisDriver : Driver, IAxisDriver
    {
        private readonly object _threadLock = new object();
        private readonly ManualResetEventSlim _connectDone = new ManualResetEventSlim();
        private Socket _commandSocket;

        public XenaxAxisDriver(IPAddress ip, ushort port)
        {
            IP = ip;
            Port = port;
            Type = nameof(XenaxAxisDriver);
            Id = $"{Type}[{ip}:{port}]";
        }

        ~XenaxAxisDriver()
        {
            Disconnect();
        }

        public enum DriverStatus
        {
            UNKNOWN_ERROR = 0,
            ERROR_IN_QUEUE = 1,
            DRIVE_IS_ACTIVE = 3,
            PROGRAM_IS_ACTIVE = 5,
            EE1_IN_QUEUE = 13,
            EE_IN_QUEUE = 14,
            FORCE_CALIBRATION_ACTIVE = 15,
            ROTARY_REFERENCE_ACTIVE = 34,
            GANTRY_REFERENCE_ACTIVE = 36,
            REFERENCE_ACTIVE = 38,
            COMMAND_AT_ACTIVE_BUS_MODULE_NOT_ALLOWED = 40,
            FAULT_REACTION_ACTIVE = 47,
            VALUE_OF_PARAMETER_NOT_VALID = 65,
            COMMAND_NOT_COMPLETED_CORRECTLY = 66
        }

        public enum AxisStatus
        {
            ERROR, HOME, IN_MOTION, IN_POSITION,
            END_OF_PROGRAM, IN_FORCE, IN_SECTOR, FORCE_IN_SECTOR,
            INVERTER_VOLTAGE, END_OF_GANTRY_INIT, LIMIT_SWITCH_LEFT, LIMIT_SWITCH_RIGHT,
            EMERGENCY_EXIT_1_REMAIN_POWER_ON, EMERGENCY_EXT_POWER_OFF, COGGING_REFERENCE_DRIVE_ACTIVE, I_FORCE_LIMIT_REACHED,
            STO_PRIMED_OR_HIT, SS1_PRIMED_OR_HIT, SS2_PRIMED, SS2_HIT,
            SLS_PRIMED, SLS_SPEED_HIT, SLS_POSITION_HIT, WARNING,
            INFORMATION, PHASING_DONE
        }

        public override bool Connected { get => _commandSocket != null ? _commandSocket.Connected : false; }

        public IPAddress IP { get; private set; }

        public ushort Port { get; private set; }

        public override bool Connect()
        {
            Disconnect();

            IPEndPoint commandEndPoint = new IPEndPoint(IP, Port);
            _commandSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _commandSocket.SendTimeout = 250;
            _commandSocket.ReceiveTimeout = 250;

            SocketAsyncEventArgs se = new SocketAsyncEventArgs
            {
                RemoteEndPoint = commandEndPoint,
                UserToken = _commandSocket
            };

            se.Completed += (sender, e) => _connectDone.Set();

            if (_commandSocket.ConnectAsync(se))
            {
                _connectDone.Reset();
                if (!_connectDone.Wait(250))
                {
                    _commandSocket.Close();
                }
            }

            return Connected;
        }

        public override async Task<bool> ConnectAsync(CancellationToken token)
        {
            Disconnect();

            IPEndPoint commandEndPoint = new IPEndPoint(IP, Port);
            _commandSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _commandSocket.SendTimeout = 250;
            _commandSocket.ReceiveTimeout = 250;

            bool isCompleted = false;
            SocketAsyncEventArgs se = new SocketAsyncEventArgs
            {
                RemoteEndPoint = commandEndPoint,
                UserToken = _commandSocket
            };

            se.Completed += (sender, e) => isCompleted = true;

            if (_commandSocket.ConnectAsync(se))
            {
                try
                {
                    while (!isCompleted)
                    {
                        await Task.Delay(10, token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    _commandSocket.Close();
                    throw;
                }
            }

            return Connected;
        }

        public override bool Disconnect()
        {
            if (_commandSocket != null)
            {
                _commandSocket.Close();
            }

            return false;
        }

        public bool IsInitialized(int axisId)
        {
            GetAxisStatus(axisId, out int axisStatus);
            IntBits status = new IntBits(axisStatus);
            return status[(int)AxisStatus.HOME];
        }

        public bool IsInMotion(int axisId)
        {
            GetAxisStatus(axisId, out int axisStatus);
            IntBits bits = new IntBits(axisStatus);
            ThrowIfErrorDetected(bits);
            return bits[(int)AxisStatus.IN_MOTION];

            void ThrowIfErrorDetected(IntBits intBits)
            {
                if (intBits[(int)AxisStatus.ERROR] && !intBits[(int)AxisStatus.HOME])
                {
                    List<string> status = new List<string>();
                    foreach (var bit in Enum.GetValues(typeof(AxisStatus)).Cast<AxisStatus>())
                    {
                        if (intBits[(int)bit])
                        {
                            status.Add(bit.ToString());
                        }
                    }

                    throw new Exception($"AxisId: {axisId} on {Id} error: {string.Join("|", status)}");
                }
            }
        }

        public void Initialize(int axisId, Direction direction)
        {
            string command = "REF";
            string echoCommand = SendCommand(command);
            ProcessEchoCommand(echoCommand, command);
        }

        public void Move(int axisId, Direction direction)
        {
            string command = null;

            switch (direction)
            {
                case Direction.Negative:
                    command = "JN";
                    break;
                case Direction.Positive:
                    command = "JP";
                    break;
            }

            string echoCommand = SendCommand(command);
            ProcessEchoCommand(echoCommand, command);
        }

        public void MoveAbs(int axisId, int absPosition)
        {
            string command = "G" + absPosition;
            string echoCommand = SendCommand(command);
            ProcessEchoCommand(echoCommand, command);
        }

        public void MoveRel(int axisId, int relativeDistance)
        {
            string command = null;
            string echoCommand = null;

            command = "WA" + relativeDistance;
            echoCommand = SendCommand(command);
            ProcessEchoCommand(echoCommand, command);

            command = "GW";
            echoCommand = SendCommand(command);
            ProcessEchoCommand(echoCommand, command);
        }

        public void Stop(int axisId, StopMode mode)
        {
            string command = "SM";
            string echoCommand = SendCommand(command);
            ProcessEchoCommand(echoCommand, command);
        }

        public void SetMovementParameter(int axisId, int initialSpeed, int finalSpeed, int accelerationDuration, int decelerationDuration, int scurveAccelerationPercentage, int scurveDecelerationPercentage)
        {
        }

        public void SetMovementParameter(int axisId, int speed, int acceleration, int scurvePercentage)
        {
            string command = null;
            string echoCommand = null;

            // Default speed is 100,000 [inc/s]
            command = "SP" + speed.ToString();
            echoCommand = SendCommand(command);
            ProcessEchoCommand(echoCommand, command);

            // Default acceleration is 1,000,000[inc/s^2]
            command = "AC" + acceleration.ToString();
            echoCommand = SendCommand(command);
            ProcessEchoCommand(echoCommand, command);

            // Default S-Curve is 20%
            command = "SCRV" + scurvePercentage.ToString();
            echoCommand = SendCommand(command);
            ProcessEchoCommand(echoCommand, command);
        }

        public void SetSoftwareLimit(int axisId, int negativeLimit, int positiveLimit)
        {
            string command = null;
            string echoCommand = null;

            command = "SLPN" + negativeLimit.ToString();
            echoCommand = SendCommand(command);
            ProcessEchoCommand(echoCommand, command);

            command = "SLPP" + positiveLimit.ToString();
            echoCommand = SendCommand(command);
            ProcessEchoCommand(echoCommand, command);
        }

        public void SetAbsPosition(int axisId, int position)
        {
        }

        public int GetAbsPosition(int axisId)
        {
            string command = "TP";
            string echoCommand = SendCommand(command);
            string processedCommand = ProcessEchoCommand(echoCommand, command);

            if (int.TryParse(processedCommand, out int position))
            {
                return position;
            }
            else
            {
                throw new InvalidOperationException(
                    $"An error occurred when attempting to GetAbsPosition on {Id}" +
                    $" failed to parse Processed Command.");
            }
        }

        public int GetAxisStatus(int axisId)
        {
            GetAxisStatus(axisId, out int axisStatus);
            return axisStatus;
        }

        public void SetAlarmLogicLevel(int axisId, bool active)
        {
        }

        public void SetServoState(int axisId, bool state)
        {
        }

        private void GetAxisStatus(int axisId, out int axisStatus)
        {
            string command = "TPSR";
            string echoCommand = SendCommand(command);
            string processedCommand = ProcessEchoCommand(echoCommand, command);

            if (!int.TryParse(
                processedCommand,
                System.Globalization.NumberStyles.HexNumber,
                null,
                out axisStatus))
            {
                throw new InvalidOperationException(
                    $"An error occurred when attempting to GetAxisStatus on {Id}" +
                    $" failed to parse Axis Status.");
            }
        }

        public string SendCommand(string inputCommand)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] sendBytes = Encoding.ASCII.GetBytes(inputCommand + "\r");
            byte[] receiveBytes = new byte[256];
            StringBuilder sb = new StringBuilder();
            string response = string.Empty;

            lock (_threadLock)
            {
                _commandSocket.Send(sendBytes);
                do
                {
                    int bytesReceived = _commandSocket.Receive(receiveBytes);
                    if (bytesReceived > 0)
                    {
                        response = Encoding.ASCII.GetString(receiveBytes, 0, bytesReceived);
                        sb.Append(response);
                    }
                }
                while (!response.Contains('>'));
            }

            return sb.ToString();
        }

        public string ProcessEchoCommand(string echoCommand, string inputCommand)
        {
            // echoCommand should contain echoed inputCommand
            if (!echoCommand.Contains(inputCommand))
            {
                throw new InvalidOperationException(
                    $"Input Command:[{inputCommand}] does not match" +
                    $"Echoed Command:[{echoCommand}] on {Id}");
            }

            // Filter output response
            string processed = echoCommand.Replace(inputCommand, string.Empty);
            Regex pattern = new Regex("[>\r\n\0]");
            processed = pattern.Replace(processed, string.Empty);

            // Echo command not recognized or cannot be completed in the current configuration
            if (processed.Contains('?'))
            {
                throw new InvalidOperationException(
                    $"Input Command:[{inputCommand}] Unknown Error on {Id}.");
            }

            // Echo command cannot be accepted at this time
            if (processed.Contains('#'))
            {
                processed = processed.Replace("#", string.Empty);
                int.TryParse(processed, out int driverStatus);
                throw new InvalidOperationException(
                    $"Input Command:[{inputCommand}] {(DriverStatus)driverStatus} on {Id}.");
            }

            return processed;
        }
    }
}
