using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using XenaxControl.Models;

namespace XenaxControl.ViewModels
{
    public class XenaxCommViewModel : ViewModelBase
    {
        private XenaxCommunication xenaxComm;
        private string connectionStatus;

        public XenaxCommViewModel()
        {
            this.xenaxComm = new XenaxCommunication();
            this.IP = "192.168.2.100";
            this.Port = "10001";
            this.ConnectionStatus = "Connect";
            this.XenaxOutput = CollectionViewSource.GetDefaultView(this.xenaxComm.XenaxOutput);

            this.WireCommands();
        }

        public string IP
        {
            get
            {
                return this.xenaxComm.IP;
            }

            set
            {
                this.xenaxComm.IP = value;
                this.OnPropertyChanged("IP");
            }
        }

        public string Port
        {
            get
            {
                return this.xenaxComm.Port;
            }

            set
            {
                this.xenaxComm.Port = value;
                this.OnPropertyChanged("Port");
            }
        }

        public string Command
        {
            get
            {
                return this.xenaxComm.Command;
            }

            set
            {
                this.xenaxComm.Command = value;
                this.OnPropertyChanged("Command");
            }
        }

        public string ConnectionStatus
        {
            get
            {
                return this.connectionStatus;
            }

            set
            {
                this.connectionStatus = value;
                this.OnPropertyChanged("ConnectionStatus");
            }
        }

        public ICollectionView XenaxOutput { get; private set; }

        public RelayCommand ConnectCommand { get; private set; }

        public RelayCommand DisconnectCommand { get; private set; }

        public RelayCommand SendCommand { get; private set; }

        public RelayCommand ProcessStatusCommand { get; private set; }

        public RelayCommand ClearCommand { get; private set; }

        private void WireCommands()
        {
            bool connectEnabled = true;

            this.ConnectCommand = new RelayCommand(
                param =>
                {
                    this.ConnectionStatus = "Connecting";
                    connectEnabled = false;
                    Task.Run(() =>
                    {
                        this.xenaxComm.Connect();
                        connectEnabled = !this.xenaxComm.Connected;
                    });
                },
                param =>
                {
                    this.ConnectionStatus = !this.xenaxComm.Connected ? "Connect" : "Connected";
                    return connectEnabled;
                });

            this.DisconnectCommand = new RelayCommand(
                param =>
                {
                    this.xenaxComm.Disconnect();
                    connectEnabled = true;
                },
                param => this.xenaxComm.Connected);

            this.SendCommand = new RelayCommand(
                param =>
                {
                    this.xenaxComm.Send();
                    this.Command = string.Empty;
                    this.XenaxOutput.Refresh();
                },
                param => this.xenaxComm.Connected);

            this.ProcessStatusCommand = new RelayCommand(
                param =>
                {
                    this.xenaxComm.GetProcessStatus();
                    this.XenaxOutput.Refresh();
                },
                param => this.xenaxComm.Connected);

            this.ClearCommand = new RelayCommand(
                param =>
                {
                    this.xenaxComm.ClearXenaxOutput();
                    this.XenaxOutput.Refresh();
                },
                param => !this.XenaxOutput.IsEmpty);
        }
    }
}
