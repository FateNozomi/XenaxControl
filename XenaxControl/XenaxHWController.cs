using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace XenaxControl
{
    public class XenaxHWController
    {
        private const double MillimetersPerUnit = 0.001;
        private XenaxHWDriver xenaxDriver;

        /// <summary>
        /// Initializes a new instance of the XenaxHWController class using the
        /// specified XenaxHWDriver.
        /// </summary>
        /// <param name="xenaxDriver">The XenaxHWDriver for this controller to communicate with.</param>
        public XenaxHWController(XenaxHWDriver xenaxDriver)
        {
            this.xenaxDriver = xenaxDriver;
        }

        public bool Initialized
        {
            get { return this.IsDriverInitialized(); }
        }

        public bool InPosition
        {
            get { return this.IsDriverInPosition(); }
        }

        public bool InMotion
        {
            get { return this.IsDriverinMotion(); }
        }

        public static double UnitToMM(double units)
        {
            return units * MillimetersPerUnit;
        }

        public static double MMToUnit(double mm)
        {
            return mm / MillimetersPerUnit;
        }

        public bool Initialize(bool blocking = true)
        {
            if (this.xenaxDriver == null)
            {
                MessageBox.Show(
                    "XENAX Driver is not linked to the controller.",
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            try
            {
                this.xenaxDriver.SendCommand("REF");

                bool timeOut = false;
                var duration = new System.Timers.Timer(3000);
                duration.Elapsed += (sender, e) => timeOut = true;
                duration.AutoReset = false;
                duration.Start();

                if (blocking)
                {
                    while (!timeOut)
                    {
                        Thread.Sleep(50);
                        if (this.Initialized)
                        {
                            break;
                        }
                    }
                }
            }
            catch (DeviceNotConnectedException dncEx)
            {
                MessageBox.Show(
                    string.Format("[{0}] Initialize Error: {1}", this.xenaxDriver.DriverName, dncEx.Message),
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("[{0}] Initialize Error: {1}", this.xenaxDriver.DriverName, ex.Message),
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        public bool Stop()
        {
            if (this.xenaxDriver == null)
            {
                MessageBox.Show(
                    "XENAX Driver is not linked to the controller.",
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            if (!this.Initialized)
            {
                MessageBox.Show(
                    string.Format("[{0}] Stop Error: Driver is not this.Initialized.", this.xenaxDriver.DriverName));
                return false;
            }

            try
            {
                this.xenaxDriver.SendCommand("SM");

                if (this.xenaxDriver.GetErrorCode() != 0)
                {
                    MessageBox.Show(
                        string.Format("[{0}] Stop Error: {1}", this.xenaxDriver.DriverName, this.xenaxDriver.GetErrorMessage()),
                        "XenaxHWController",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }
            }
            catch (DeviceNotConnectedException dncEx)
            {
                MessageBox.Show(
                    string.Format("[{0}] Stop Error: {1}", this.xenaxDriver.DriverName, dncEx.Message),
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("[{0}] Stop Error: {1}", this.xenaxDriver.DriverName, ex.Message),
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        public bool MoveAbsolute(double mm, bool blocking = true)
        {
            if (this.xenaxDriver == null)
            {
                MessageBox.Show(
                    "XENAX Driver is not linked to the controller.",
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            if (!this.Initialized)
            {
                MessageBox.Show(
                    string.Format("[{0}] MoveAbsolute Error: Driver is not this.Initialized.", this.xenaxDriver.DriverName));
                return false;
            }

            try
            {
                double units = MMToUnit(mm);
                this.xenaxDriver.SendCommand("G" + units);

                if (blocking)
                {
                    while (true)
                    {
                        Thread.Sleep(50);
                        if (!this.xenaxDriver.IsDriverInMotion())
                        {
                            break;
                        }
                    }
                }

                if (this.xenaxDriver.GetErrorCode() != 0)
                {
                    MessageBox.Show(
                        string.Format("[{0}] MoveAbsolute Error: {1}", this.xenaxDriver.DriverName, this.xenaxDriver.GetErrorMessage()),
                        "XenaxHWController",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }
            }
            catch (DeviceNotConnectedException dncEx)
            {
                MessageBox.Show(
                    string.Format("[{0}] MoveAbsolute Error: {1}", this.xenaxDriver.DriverName, dncEx.Message),
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("[{0}] MoveAbsolute Error: {1}", this.xenaxDriver.DriverName, ex.Message),
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        public bool MoveRelative(double mm, bool blocking = true)
        {
            if (this.xenaxDriver == null)
            {
                MessageBox.Show(
                    "XENAX Driver is not linked to the controller.",
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            if (!this.Initialized)
            {
                MessageBox.Show(
                    string.Format("[{0}] MoveRelative Error: Driver is not this.Initialized.", this.xenaxDriver.DriverName));
                return false;
            }

            try
            {
                double units = MMToUnit(mm);
                this.xenaxDriver.SendCommand("WA" + units);
                this.xenaxDriver.SendCommand("GW");

                if (blocking)
                {
                    while (true)
                    {
                        Thread.Sleep(50);
                        if (this.xenaxDriver.IsDriverInMotion())
                        {
                            break;
                        }
                    }
                }

                if (this.xenaxDriver.GetErrorCode() != 0)
                {
                    MessageBox.Show(
                        string.Format("[{0}] MoveRelative Error: {1}", this.xenaxDriver.DriverName, this.xenaxDriver.GetErrorMessage()),
                        "XenaxHWController",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }
            }
            catch (DeviceNotConnectedException dncEx)
            {
                MessageBox.Show(
                    string.Format("[{0}] MoveRelative Error: {1}", this.xenaxDriver.DriverName, dncEx.Message),
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("[{0}] MoveRelative Error: {1}", this.xenaxDriver.DriverName, ex.Message),
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        public bool GetPosition(ref double position)
        {
            if (this.xenaxDriver == null)
            {
                MessageBox.Show(
                    "XENAX Driver is not linked to the controller.",
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            if (!this.Initialized)
            {
                MessageBox.Show(
                    string.Format("[{0}] GetPosition Error: Driver is not this.Initialized.", this.xenaxDriver.DriverName),
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return false;
            }

            try
            {
                string receivedOutput = this.xenaxDriver.SendCommand("TP");
                if (double.TryParse(receivedOutput, out position))
                {
                    position *= MillimetersPerUnit;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (DeviceNotConnectedException dncEx)
            {
                MessageBox.Show(
                    string.Format("[{0}] GetPosition Error: {1}", this.xenaxDriver.DriverName, dncEx.Message),
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("[{0}] GetPosition Error: {1}", this.xenaxDriver.DriverName, ex.Message),
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        public bool GetSLPP(ref double position)
        {
            return this.GetValue("SLPP?", ref position);
        }

        public bool GetSLPN(ref double position)
        {
            return this.GetValue("SLPN?", ref position);
        }

        public bool GetAcceleration(ref double value)
        {
            return this.GetValue("AC?", ref value);
        }

        public bool GetSpeed(ref double value)
        {
            return this.GetValue("SP?", ref value);
        }

        public bool GetSCurve(ref double value)
        {
            return this.GetValue("SCRV?", ref value);
        }

        public bool GetSPOVRD(ref double value)
        {
            return this.GetValue("OVRD?", ref value);
        }

        private bool GetValue(string command, ref double value)
        {
            if (this.xenaxDriver == null)
            {
                MessageBox.Show(
                    "XENAX Driver is not linked to the controller.",
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            if (!this.Initialized)
            {
                return false;
            }

            try
            {
                string receivedOutput = this.xenaxDriver.SendCommand(command);
                if (double.TryParse(receivedOutput, out value))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (DeviceNotConnectedException dncEx)
            {
                MessageBox.Show(
                    string.Format("[{0}] {1} Error: {2}", this.xenaxDriver.DriverName, command, dncEx.Message),
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("[{0}] {2} Error: {1}", this.xenaxDriver.DriverName, command, ex.Message),
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool IsDriverInitialized()
        {
            try
            {
                if (this.xenaxDriver == null)
                {
                    MessageBox.Show(
                        "XENAX Driver is not linked to the controller.",
                        "XenaxHWController",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                return this.xenaxDriver.IsDriverInitialized();
            }
            catch (DeviceNotConnectedException dncEx)
            {
                MessageBox.Show(
                    string.Format("[{0}] isDriverInitialized Error: {1}", this.xenaxDriver.DriverName, dncEx.Message),
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("[{0}] isDriverInitialized Error: {1}", this.xenaxDriver.DriverName, ex.Message),
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool IsDriverInPosition()
        {
            try
            {
                if (this.xenaxDriver == null)
                {
                    MessageBox.Show(
                        "XENAX Driver is not linked to the controller.",
                        "XenaxHWController",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                return this.xenaxDriver.IsDriverInPosition();
            }
            catch (DeviceNotConnectedException dncEx)
            {
                MessageBox.Show(
                    string.Format("[{0}] isDriverInPosition Error: {1}", this.xenaxDriver.DriverName, dncEx.Message),
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("[{0}] isDriverInPosition Error: {1}", this.xenaxDriver.DriverName, ex.Message),
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool IsDriverinMotion()
        {
            try
            {
                if (this.xenaxDriver == null)
                {
                    MessageBox.Show(
                        "XENAX Driver is not linked to the controller.",
                        "XenaxHWController",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                return this.xenaxDriver.IsDriverInMotion();
            }
            catch (DeviceNotConnectedException dncEx)
            {
                MessageBox.Show(
                    string.Format("[{0}] isDriverinMotion Error: {1}", this.xenaxDriver.DriverName, dncEx.Message),
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("[{0}] isDriverinMotion Error: {1}", this.xenaxDriver.DriverName, ex.Message),
                    "XenaxHWController",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }
    }
}
