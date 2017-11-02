using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace XenaxControl.Models
{
    public class XenaxCommunication
    {
        private XenaxHWDriver xenaxDriver;

        public XenaxCommunication()
        {
            this.XenaxOutput = new List<string>();
        }

        public string IP { get; set; }

        public string Port { get; set; }

        public string Command { get; set; }

        public List<string> XenaxOutput { get; set; }

        public bool Connected
        {
            get
            {
                if (this.xenaxDriver == null)
                {
                    return false;
                }

                return this.xenaxDriver.Connected;
            }
        }

        public void Connect()
        {
            if (string.IsNullOrEmpty(this.IP))
            {
                MessageBox.Show("Please enter a valid IP address.", "XENAX", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (string.IsNullOrEmpty(this.Port))
            {
                MessageBox.Show("Please enter a valid Port.", "XENAX", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            string[] arrIP = this.IP.Split('.');
            byte[] byteIP;
            int port = 0;

            try
            {
                byteIP = this.ValidateIP(arrIP);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "XENAX", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            try
            {
                port = Convert.ToInt32(this.Port);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid XENAX driver Port: " + ex.Message, "XENAX", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            this.xenaxDriver = new XenaxHWDriver(string.Empty, byteIP, port);

            try
            {
                this.xenaxDriver.Connect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "XENAX", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        public void Disconnect()
        {
            if (this.xenaxDriver != null)
            {
                this.xenaxDriver.Disconnect();
            }
        }

        public void Send()
        {
            if (this.xenaxDriver == null)
            {
                MessageBox.Show("XENAX Driver is not connected.", "XENAX", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            try
            {
                string output = this.xenaxDriver.SendCommand(this.Command);
                this.XenaxOutput.Add(string.Format("> {0}", output));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "XENAX", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        public void GetProcessStatus()
        {
            if (this.xenaxDriver == null)
            {
                MessageBox.Show("XENAX Driver is not connected.", "XENAX", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            try
            {
                string output = this.xenaxDriver.SendCommand("TPSR");
                IntBits tpsr = new IntBits(Convert.ToInt32(output, 16));
                foreach (XenaxHWDriver.ProcessStatus processStatus in Enum.GetValues(typeof(XenaxHWDriver.ProcessStatus)))
                {
                    if (tpsr[(int)processStatus])
                    {
                        output += string.Format("\r\n\t{0}", processStatus);
                    }
                }

                this.XenaxOutput.Add(string.Format("> {0}", output));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "XENAX", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        public void ClearXenaxOutput()
        {
            this.XenaxOutput.Clear();
        }

        private byte[] ValidateIP(string[] stringIP)
        {
            byte[] ip = new byte[stringIP.Length];
            if (stringIP.Length != 4)
            {
                throw new ArgumentException("IP address format is invalid. Please key in the correct IP address of XENAX driver.");
            }

            for (int i = 0; i < stringIP.Length; i++)
            {
                ip[i] = Convert.ToByte(stringIP[i]);
            }

            return ip;
        }
    }
}
