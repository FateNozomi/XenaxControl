using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XenaxControl.Models;

namespace XenaxControl.ViewModels
{
    public class XenaxCommViewModel : ObservableObject
    {
        private ObservableCollection<string> xenaxOutput;
        private string stringIP;
        private string stringPort;
        private string stringCommand;
        private bool isConnectEnabled;
        private bool isDisconnectEnabled;
        private XenaxHWDriver xenaxDriver;

        public XenaxCommViewModel()
        {
            this.xenaxOutput = new ObservableCollection<string>();
            this.stringIP = "192.168.2.100";
            this.stringPort = "10001";
            this.isConnectEnabled = true;
            this.isDisconnectEnabled = false;
        }

        public string StringIP
        {
            get
            {
                return this.stringIP;
            }

            set
            {
                this.stringIP = value;
                this.NotifyPropertyChanged("StringIP");
            }
        }

        public string StringPort
        {
            get
            {
                return this.stringPort;
            }

            set
            {
                this.stringPort = value;
                this.NotifyPropertyChanged("StringPort");
            }
        }

        public string StringCommand
        {
            get
            {
                return this.stringCommand;
            }

            set
            {
                this.stringCommand = value;
                this.NotifyPropertyChanged("StringCommand");
            }
        }

        public bool IsConnectEnabled
        {
            get
            {
                return this.isConnectEnabled;
            }

            set
            {
                this.isConnectEnabled = value;
                this.NotifyPropertyChanged("IsConnectEnabled");
            }
        }

        public bool IsDisconnectEnabled
        {
            get
            {
                return this.isDisconnectEnabled;
            }

            set
            {
                this.isDisconnectEnabled = value;
                this.NotifyPropertyChanged("IsDisconnectEnabled");
            }
        }
        
        public ObservableCollection<string> XenaxOutput
        {
            get
            {
                return this.xenaxOutput;
            }

            set
            {
                this.xenaxOutput = value;
                this.NotifyPropertyChanged("XenaxOutput");
            }
        }

        public ICommand ConnectCommand
        {
            get { return new Command(this.Connect); }
        }

        public ICommand DisconnectCommand
        {
            get { return new Command(this.Disconnect); }
        }

        public ICommand SendCommand
        {
            get { return new Command(this.Send); }
        }

        private void Connect()
        {
            if (string.IsNullOrEmpty(this.stringIP))
            {
                MessageBox.Show("Please enter a valid IP address.", "XENAX", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (string.IsNullOrEmpty(this.StringPort))
            {
                MessageBox.Show("Please enter a valid Port.", "XENAX", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            string[] stringIPArr = this.stringIP.Split('.');
            byte[] ip;
            int port = 0;

            try
            {
                ip = this.ValidateIP(stringIPArr);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "XENAX", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            try
            {
                port = Convert.ToInt32(this.stringPort);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid XENAX driver Port: " + ex.Message, "XENAX", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            this.xenaxDriver = new XenaxHWDriver(string.Empty, ip, port);

            //Task<bool> isConnected = Task.Run(() => 
            //{
            //    try
            //    {
            //        return this.xenaxDriver.Connect();
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(ex.Message, "XENAX", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //        return false;
            //    }
            //});
            bool isConnected = false;

            try
            {
                isConnected = this.xenaxDriver.Connect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "XENAX", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            if (isConnected)
            {
                this.IsConnectEnabled = false;
                this.IsDisconnectEnabled = true;
            }
            else
            {
                this.isConnectEnabled = true;
                this.IsDisconnectEnabled = false;
            }
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

        private void Disconnect()
        {
            if (this.xenaxDriver != null)
            {
                this.xenaxDriver.Disconnect();
            }

            this.IsConnectEnabled = true;
            this.IsDisconnectEnabled = false;
        }

        private void Send()
        {
            if (this.xenaxDriver == null)
            {
                MessageBox.Show("XENAX Driver is not connected.", "XENAX", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            try
            {
                string output = this.xenaxDriver.SendCommand(this.StringCommand);
                this.xenaxOutput.Add(string.Format("> {0}\r\n", output));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "XENAX", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
}
