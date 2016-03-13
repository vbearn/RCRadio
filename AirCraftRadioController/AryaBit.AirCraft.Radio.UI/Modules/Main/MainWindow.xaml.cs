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
        private Joystick joystick;

        #endregion

        #region init

        public MainWindow()
        {
            InitializeComponent();

            AddLogItem("SYS", "App Started.");
            initModules();

        }

        private void initModules()
        {
            AddLogItem("SYS", "Initializing modules...");
            logItems = new List<LogItem>();

            try
            {
                AddLogItem("SYS", "Initializing Joystick module...");
                initJoystic();
                joyLeft.YValue = 90;
            }
            catch (Exception)
            {
                AddLogItem("SYS", "Joystick module initialization FAILED.");
            }

            AddLogItem("SYS", "Modules initialized.");
        }

        #endregion

        #region Radio

        private object radioThreadLock = new object();

        private void btnComConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.rCommand = new Core.RadioCommands();
                this.rCommand.LogOccured += RCommand_LogOccured;
                this.rCommand.initComPort();
                btnComConnect.IsEnabled = false;
                btnComDisconnect.IsEnabled = true;
            }
            catch (Exception ex)
            {
                this.rCommand = null;
                ShowError("COM connection failed.");
            }

        }

        private void btnComDisconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.rCommand.CloseComPort();
                this.rCommand = null;
                btnComConnect.IsEnabled = true;
                btnComDisconnect.IsEnabled = false;
            }
            catch (Exception ex)
            {
                ShowError("COM disconnect failed.");
            }
        }

        private void RCommand_LogOccured(string arg1, string arg2)
        {
            AddLogItem(arg1, arg2);
        }

        #endregion

        #region Joystick

        int lastY;
        int lastX;
        int lastRY;
        int lastRX;

        private void initJoystic()
        {
            joystick = new Joystick();
            joystick.OnStateUpdated += Joystick_OnStateUpdated;
            joystick.StartCapture();
        }

        private void Joystick_OnStateUpdated(SlimDX.DirectInput.JoystickState state)
        {

            AircraftState aircraftState = new AircraftState()
            {
                rudderValue = (byte)(state.X * 180 / 65533),
                aileronValue = (byte)(state.RotationX * 180 / 65533),
                elevatorValue = (byte)(state.Y * 180 / 65533),
                throtleValue = (byte)(state.RotationY * 180 / 65533),

            };

            if (this.rCommand != null)
                this.rCommand.SetAircraftValues(aircraftState);

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {

                if (state.X != lastX)
                {
                    lastX = state.X;
                    sliderX.Value = aircraftState.rudderValue;
                    joyLeft.XValue = aircraftState.rudderValue;
                }

                if (state.Y != lastY)
                {
                    lastY = state.Y;
                    sliderY.Value = aircraftState.elevatorValue;
                    joyLeft.YValue = aircraftState.elevatorValue;

                }

                if (state.RotationX != lastRX)
                {
                    lastRX = state.RotationX;
                    sliderRX.Value = aircraftState.aileronValue;
                    joyRight.XValue = aircraftState.aileronValue;
                }

                if (state.RotationY != lastRY)
                {
                    lastRY = state.RotationY;
                    sliderRY.Value = aircraftState.throtleValue;
                    joyRight.YValue = aircraftState.throtleValue;
                }

                if (this.rCommand != null)
                    this.textBox.Text = this.rCommand.FPS.ToString();
            }));


        }

        private void btnStartSendJoystic_Click(object sender, RoutedEventArgs e)
        {
            rCommand.StartSendingThread();
            btnStartSendJoystic.IsEnabled = false;
            btnStopSendJoystic.IsEnabled = true;
        }

        private void btnStopSendJoystic_Click(object sender, RoutedEventArgs e)
        {
            rCommand.StopSendingThread();
            btnStartSendJoystic.IsEnabled = true;
            btnStopSendJoystic.IsEnabled = false;
        }

        #endregion

        #region Log

        private void ShowError(string error)
        {
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
