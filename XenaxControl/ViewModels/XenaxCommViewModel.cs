using Framework.MVVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Data;
using XenaxControl.Hardware.Drivers;
using XenaxControl.Utilities;

namespace XenaxControl.ViewModels
{
    public class XenaxCommViewModel : PropertyChangedBase
    {
        private List<string> _xenaxOutput = new List<string>();
        private XenaxAxisDriver _driver;

        public XenaxCommViewModel()
        {
            IP = "192.168.2.100";
            Port = 10001;
            XenaxOutput = CollectionViewSource.GetDefaultView(_xenaxOutput);
            WireCommands();
        }

        public string IP { get; set; }

        public ushort Port { get; set; }

        public string Command { get; set; }

        public ICollectionView XenaxOutput { get; private set; }

        public AsyncCommand<object> ConnectCommand { get; private set; }

        public RelayCommand DisconnectCommand { get; private set; }

        public RelayCommand SendCommand { get; private set; }

        public RelayCommand ProcessStatusCommand { get; private set; }

        public RelayCommand ClearCommand { get; private set; }

        private void WireCommands()
        {
            ConnectCommand = AsyncCommand.Create(
                async (token, param) =>
                {
                    try
                    {
                        IPAddress address = IPAddress.Parse(IP);
                        _driver = new XenaxAxisDriver(address, Port);
                        await _driver.ConnectAsync(token);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                },
                param => _driver?.Connected != true);

            DisconnectCommand = new RelayCommand(
                param =>
                {
                    _driver.Disconnect();
                },
                param => _driver?.Connected == true);

            SendCommand = new RelayCommand(
                param =>
                {
                    string output = _driver.SendCommand(Command);
                    _xenaxOutput.Add(string.Format("> {0}", output));
                    XenaxOutput.Refresh();
                },
                param => _driver?.Connected == true);

            ProcessStatusCommand = new RelayCommand(
                param =>
                {
                    try
                    {
                        string echoCommand = _driver.SendCommand("TPSR");
                        string processedCommand = _driver.ProcessEchoCommand(echoCommand, "TPSR");

                        if (!int.TryParse(
                            processedCommand,
                            System.Globalization.NumberStyles.HexNumber,
                            null,
                            out int axisStatus))
                        {
                            throw new InvalidOperationException(
                                $"An error occurred when attempting to GetAxisStatus on {IP}" +
                                $" failed to parse Axis Status.");
                        }

                        string output = string.Empty;
                        IntBits status = new IntBits(axisStatus);
                        foreach (XenaxAxisDriver.AxisStatus processStatus in Enum.GetValues(typeof(XenaxAxisDriver.AxisStatus)))
                        {
                            if (status[(int)processStatus])
                            {
                                output += string.Format("\r\n\t{0}", processStatus);
                            }
                        }

                        _xenaxOutput.Add(string.Format("> {0}", output));
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }

                    XenaxOutput.Refresh();
                },
                param => _driver?.Connected == true);

            ClearCommand = new RelayCommand(
                param =>
                {
                    _xenaxOutput.Clear();
                    XenaxOutput.Refresh();
                },
                param => _driver?.Connected == true);
        }
    }
}
