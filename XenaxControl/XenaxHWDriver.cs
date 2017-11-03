using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace XenaxControl
{
    public class XenaxHWDriver : BaseHWDriver, IDriver
    {
        private static int driverCount;
        private string driverName;
        private IPAddress ip;
        private int port;
        private Socket commandSocket;
        private object threadLock;

        public XenaxHWDriver(string driverName, byte[] ip, int port)
        {
            this.driverName = driverName;
            this.ip = new IPAddress(ip);
            this.port = port;
            this.Connected = false;
            this.threadLock = new object();
            driverCount++;
        }

        ~XenaxHWDriver()
        {
            this.Disconnect();
        }

        #region Process Status
        public enum ProcessStatus
        {
            ERROR, HOME, IN_MOTION, IN_POSITION,
            END_OF_PROGRAM, IN_FORCE, IN_SECTOR, FORCE_IN_SECTOR,
            INVERTER_VOLTAGE, END_OF_GANTRY_INIT, LIMIT_SWITCH_LEFT, LIMIT_SWITCH_RIGHT,
            EMERGENCY_EXIT_1_REMAIN_POWER_ON, EMERGENCY_EXT_POWER_OFF, COGGING_REFERENCE_DRIVE_ACTIVE, I_FORCE_LIMIT_REACHED,
            STO_PRIMED_OR_HIT, SS1_PRIMED_OR_HIT, SS2_PRIMED, SS2_HIT,
            SLS_PRIMED, SLS_SPEED_HIT, SLS_POSITION_HIT, WARNING,
            INFORMATION, PHASING_DONE
        }
        #endregion

        public override bool Connected
        {
            get
            {
                lock (this.threadLock)
                {
                    this.IsDeviceConnected();
                }

                return base.Connected;
            }
        }

        public int DriverCount
        {
            get { return driverCount; }
        }

        public string DriverName
        {
            get { return this.driverName; }
        }

        public override bool Connect()
        {
            IPEndPoint commandEndPoint = new IPEndPoint(this.ip, this.port);

            this.commandSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.commandSocket.Connect(commandEndPoint);
            this.Connected = this.commandSocket.Connected;

            return this.Connected;
        }

        public async Task<bool> ConnectAsync(CancellationToken token)
        {
            IPEndPoint commandEndPoint = new IPEndPoint(this.ip, this.port);
            this.commandSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            bool operationComplete = false;
            SocketAsyncEventArgs se = new SocketAsyncEventArgs();
            se.RemoteEndPoint = commandEndPoint;
            se.UserToken = this.commandSocket;
            se.Completed += (sender, e) => operationComplete = true;

            this.commandSocket.ConnectAsync(se);

            await Task.Run(() =>
            {
                while (!operationComplete)
                {
                    Task.Delay(100, token).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                }
            }).ConfigureAwait(false);

            return this.Connected;
        }

        public override bool Disconnect()
        {
            if (this.commandSocket != null)
            {
                if (this.commandSocket.Connected)
                {
                    this.commandSocket.Close();
                }

                this.commandSocket = null;
            }

            return base.Disconnect();
        }

        public string SendCommand(string inputCommand)
        {
            if (!this.Connected)
            {
                throw new DeviceNotConnectedException(string.Format("XenaxHWDriver is not connected."));
            }

            string recvStr = string.Empty;
            byte[] sendBytes = ASCIIEncoding.ASCII.GetBytes(inputCommand + "\r");
            byte[] recvBytes = new byte[256];

            this.commandSocket.Send(sendBytes);
            this.commandSocket.Receive(recvBytes);
            recvStr = UTF8Encoding.UTF8.GetString(recvBytes);
            recvStr = recvStr.Replace(inputCommand, string.Empty);
            Regex pattern = new Regex("[>\r\n\0]");
            recvStr = pattern.Replace(recvStr, string.Empty);

            return recvStr;
        }

        public bool IsDriverInitialized()
        {
            string hexOutput = this.SendCommand("TPSR");
            IntBits tpsr = new IntBits(Convert.ToInt32(hexOutput, 16));
            return tpsr[(int)ProcessStatus.PHASING_DONE];
        }

        public bool IsDriverInPosition()
        {
            string hexOutput = this.SendCommand("TPSR");
            IntBits tpsr = new IntBits(Convert.ToInt32(hexOutput, 16));
            return tpsr[(int)ProcessStatus.IN_POSITION];
        }

        public bool IsDriverInMotion()
        {
            string hexOutput = this.SendCommand("TPSR");
            IntBits tpsr = new IntBits(Convert.ToInt32(hexOutput, 16));
            return tpsr[(int)ProcessStatus.IN_MOTION];
        }

        public int GetErrorCode()
        {
            return Convert.ToInt32(this.SendCommand("TE"));
        }

        public string GetErrorMessage()
        {
            return this.SendCommand("TES");
        }

        private void IsDeviceConnected()
        {
            if (this.commandSocket == null)
            {
                this.Connected = false;
                return;
            }

            bool blockingState = this.commandSocket.Blocking;
            try
            {
                byte[] tmp = new byte[1];

                this.commandSocket.Blocking = false;
                this.commandSocket.Send(tmp, 0, SocketFlags.None);
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                {
                    Console.WriteLine("Still Connected, but the Send would block");
                    this.Connected = this.commandSocket.Connected;
                    return;
                }
                else
                {
                    Console.WriteLine("Disconnected: error code {0}!", e.NativeErrorCode);
                    this.Connected = false;
                    return;
                }
            }
            finally
            {
                this.commandSocket.Blocking = blockingState;
            }

            this.Connected = this.commandSocket.Connected;
        }
    }
}
