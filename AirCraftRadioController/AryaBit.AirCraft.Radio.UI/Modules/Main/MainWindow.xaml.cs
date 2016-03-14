using AryaBit.AirCraft.Radio.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AryaBit.AirCraft.Radio.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Fields

        List<LogItem> logItems;

        private Radio.Core.RadioCommands rCommand;
        private SerialTransmitter comTransmitter;
        private bool comInitialized = false;

        private Joystick joystick;
        private bool joystickInitialized = false;
        private bool joystickSending = false;

        private AircraftState lastAircraftState;

        #endregion

        #region init

        public MainWindow()
        {
            InitializeComponent();

            initModules();

        }

        private void initModules()
        {
            AddLogItem("SYS", "App Started.");
            lastAircraftState = new AircraftState(0, 0, 0, 0);

            AddLogItem("SYS", "Initializing modules...");
            logItems = new List<LogItem>();

            try
            {
                AddLogItem("SYS", "Initializing Joystick module...");
                initJoystic();
                joyLeft.XValue = 90;
                joyLeft.YValue = 90;
                joyRight.XValue = 90;
                joyRight.YValue = 90;
            }
            catch (Exception)
            {
                AddLogItem("ERR", "Joystick module initialization FAILED.");
            }

            AddLogItem("SYS", "Modules initialized.");
        }

        #endregion

        #region Radio

        private void btnComConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] ports = SerialTransmitter.GetPortNames();
                string portToConnect;
                if (ports.Length == 1)
                {
                    portToConnect = ports[0];
                }
                else
                {
                    ShowError("COM connect failed. More than 1 COM port!");
                    return;
                }

                this.comTransmitter = new SerialTransmitter(portToConnect);
                this.comTransmitter.DateReceived += ComTransmitter_DateReceived;
                this.comTransmitter.Open();
            }
            catch (Exception ex)
            {
                this.rCommand = null;
                ShowError("COM connection failed.");
                return;
            }


            this.rCommand = new Core.RadioCommands(comTransmitter);
            this.rCommand.LogOccured += RCommand_LogOccured;

            this.comInitialized = true;
            SetUIButtonsLayout();
        }

        private void ComTransmitter_DateReceived(string connLog)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                txtConnLog.Text += connLog;
            }));
        }

        private void btnComDisconnect_Click(object sender, RoutedEventArgs e)
        {
            this.comInitialized = false;

            try
            {
                this.comTransmitter.Close();

            }
            catch (Exception ex)
            {
                ShowError("COM disconnect failed.");
            }


            this.rCommand = null;
            SetUIButtonsLayout();
        }

        private void RCommand_LogOccured(string chan, string value)
        {
            AddLogItem(chan, value);
        }

        #endregion

        #region Joystick

        private void initJoystic()
        {
            this.joystick = new Joystick();
            this.joystick.OnStateUpdated += Joystick_OnStateUpdated;
            this.joystick.StartCapture();
            this.joystickInitialized = true;
        }

        private void Joystick_OnStateUpdated(SlimDX.DirectInput.JoystickState state)
        {

            if (!this.joystickInitialized)
                return;

            AircraftState aircraftState = new AircraftState()
            {
                rudderValue = (byte)(state.X * 180 / 65533),
                aileronValue = (byte)(state.RotationX * 180 / 65533),
                elevatorValue = (byte)(state.Y * 180 / 65533),
                throtleValue = (byte)(state.RotationY * 180 / 65533),

            };

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                SetControllerValues(aircraftState);
            }));

            if (!this.joystickSending)
                return;

            if (this.rCommand != null)
                this.rCommand.SetAircraftValues(aircraftState);

        }

        private void btnStartSendJoystic_Click(object sender, RoutedEventArgs e)
        {
            if (!this.joystickInitialized)
            {
                ShowError("Joystick has not been initialized.");
                return;
            }

            AddLogItem("SYS", "Starting to sending joystick values...");
            this.rCommand.StartSendingThread();

            this.joystickSending = true;

            SetUIButtonsLayout();
        }

        private void btnStopSendJoystic_Click(object sender, RoutedEventArgs e)
        {
            if (rCommand != null)
                this.rCommand.StopSendingThread();

            this.joystickSending = false;

            SetUIButtonsLayout();

            AddLogItem("SYS", "Stopped sending joystick values.");
        }

        #endregion

        #region UI

        private void SetControllerValues(AircraftState aircraftState)
        {

            try
            {
                if (aircraftState.rudderValue != this.lastAircraftState.rudderValue)
                {
                    sliderX.Value = aircraftState.rudderValue;
                    joyLeft.XValue = aircraftState.rudderValue;
                }

                if (aircraftState.elevatorValue != this.lastAircraftState.elevatorValue)
                {
                    sliderY.Value = aircraftState.elevatorValue;
                    joyLeft.YValue = aircraftState.elevatorValue;

                }

                if (aircraftState.aileronValue != this.lastAircraftState.aileronValue)
                {
                    sliderRX.Value = aircraftState.aileronValue;
                    joyRight.XValue = aircraftState.aileronValue;
                }

                if (aircraftState.throtleValue != this.lastAircraftState.throtleValue)
                {
                    sliderRY.Value = aircraftState.throtleValue;
                    joyRight.YValue = aircraftState.throtleValue;
                }

                this.lastAircraftState = aircraftState;

                if (this.rCommand != null)
                    this.textBox.Text = this.rCommand.FPS.ToString();
            }
            catch (Exception ex)
            {
                AddLogItem("ERR", "Error in SetControllerValues" + ex.ToString());
            }

        }

        private void SetUIButtonsLayout()
        {

            btnComConnect.IsEnabled = !this.comInitialized;
            btnComDisconnect.IsEnabled = this.comInitialized;

            btnStartSendJoystic.IsEnabled = !this.joystickSending;
            btnStopSendJoystic.IsEnabled = this.joystickSending;

        }

        #endregion

        #region Log

        private void ShowError(string error)
        {
            AddLogItem("ERR", error);
            MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void AddLogItem(string channel, string value)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (listView.Items.Count > 10000)
                    listView.Items.Clear();

                listView.Items.Add(new LogItem() { Time = DateTime.Now.ToString(), Channel = channel, Value = value });
                listView.SelectedIndex = listView.Items.Count - 1;

                listView.ScrollIntoView(listView.Items[listView.SelectedIndex]);
            }));

        }

        #endregion

        #region Test
        private void btnTestChan1_Click(object sender, RoutedEventArgs e)
        {
            SendChannelTestData(1);
        }

        private void btnTestChan2_Click(object sender, RoutedEventArgs e)
        {
            SendChannelTestData(2);
        }

        private void btnTestChan3_Click(object sender, RoutedEventArgs e)
        {
            SendChannelTestData(3);
        }

        private void btnTestChan4_Click(object sender, RoutedEventArgs e)
        {
            SendChannelTestData(4);
        }

        private void SendChannelTestData(byte channelNumber)
        {
            if (rCommand == null)
                return;
            new Thread(() =>
            {
                rCommand.SendChannelData(channelNumber, 0);
                System.Threading.Thread.Sleep(1000);
                rCommand.SendChannelData(channelNumber, 10);
                System.Threading.Thread.Sleep(1000);
                rCommand.SendChannelData(channelNumber, 150);
                System.Threading.Thread.Sleep(1000);
                rCommand.SendChannelData(channelNumber, 100);
                System.Threading.Thread.Sleep(1000);
                rCommand.SendChannelData(channelNumber, 10);
            }).Start();
        }

        #endregion

    }

    public class LogItem
    {
        public string Time { get; set; }

        public string Channel { get; set; }

        public string Value { get; set; }
    }
}
