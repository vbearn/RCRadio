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

        //**** UI ****
        private System.Timers.Timer timerUI;
        private bool windowIsMoving;
        private DateTime windowDragEndTime;

        private AircraftState lastAircraftState;

        #endregion

        #region init

        public MainWindow()
        {
            InitializeComponent();

            initControlsDefaults();

            AircraftRadioLogic.Instance.LogOccured += (logItem) => { AddLogItem(logItem); };
            AircraftRadioLogic.Instance.ConnectionLogOccured += (logItem) => { AddConnectionLogItem(logItem); };
            AircraftRadioLogic.Instance.NeedUpdateUI += () => { SetUIButtonsLayout(); };

            AircraftRadioLogic.Instance.initModules();
            LoadSettingsToUI();

            initJoystickBoundries(AircraftRadioLogic.Instance.RadioConfig);

        }

        private void initControlsDefaults()
        {
            var dragExculdedUIElements = new UIElement[] {

            };

            //DragHandler.DragMoveWindow(this, null,
            //    () =>
            //    {
            //        //DisableFollowCursorOnMove();
            //        this.windowIsMoving = true;
            //    },
            //    (hasMoved) =>
            //    {
            //        this.windowIsMoving = false;
            //        this.windowDragEndTime = DateTime.Now;
            //    },
            //    dragExculdedUIElements);


            this.lastAircraftState = new AircraftState();

            joyLeft.XValue = 90;
            joyLeft.YValue = 90;
            joyRight.XValue = 90;
            joyRight.YValue = 90;

            timerUI = new System.Timers.Timer();
            timerUI.Interval = 100;
            timerUI.Elapsed += timerUI_Elapsed;
            timerUI.Start();

        }

        private void initJoystickBoundries(RadioConfigurations radioConfig)
        {
            joyLeft.MaxXValue = radioConfig.Rudder_MaxValue - radioConfig.Rudder_MinValue;
            joyLeft.MaxYValue = radioConfig.Throtle_MaxValue - radioConfig.Throtle_MinValue;

            joyRight.MaxXValue = radioConfig.Aileron_MaxValue - radioConfig.Aileron_MinValue;
            joyRight.MaxYValue = 65533;
        }

        #endregion

        #region Settings

        private bool disableSaveSettings = false;

        private void LoadSettingsToUI()
        {
            this.disableSaveSettings = true;

            RadioConfigurations radioConfig = AircraftRadioLogic.Instance.RadioConfig;

            sldElevator_MinValue1.Value = radioConfig.Elevator_MinValue1;
            sldElevator_MinValue2.Value = radioConfig.Elevator_MinValue2;
            sldElevator_MaxValue1.Value = radioConfig.Elevator_MaxValue1;
            sldElevator_MaxValue2.Value = radioConfig.Elevator_MaxValue2;
            sldElevator_Offset1.Value = radioConfig.Elevator_Offset1;
            sldElevator_Offset2.Value = radioConfig.Elevator_Offset2;
            chkElevator_Reverse1.IsChecked = radioConfig.Elevator_Reverse1;
            chkElevator_Reverse2.IsChecked = radioConfig.Elevator_Reverse2;

            sldAileron_Offset1.Value = radioConfig.Aileron_Offset1;
            sldAileron_Offset2.Value = radioConfig.Aileron_Offset2;

            this.disableSaveSettings = false;

        }


        private void SaveSettingsFromUI()
        {
            if (this.disableSaveSettings)
                return;

            if (AircraftRadioLogic.Instance.RadioConfig != null)
            {
                AircraftRadioLogic.Instance.RadioConfig.Elevator_MinValue1 = (byte)sldElevator_MinValue1.Value;
                AircraftRadioLogic.Instance.RadioConfig.Elevator_MinValue2 = (byte)sldElevator_MinValue2.Value;
                AircraftRadioLogic.Instance.RadioConfig.Elevator_MaxValue1 = (byte)sldElevator_MaxValue1.Value;
                AircraftRadioLogic.Instance.RadioConfig.Elevator_MaxValue2 = (byte)sldElevator_MaxValue2.Value;
                AircraftRadioLogic.Instance.RadioConfig.Elevator_Offset1 = (int)sldElevator_Offset1.Value;
                AircraftRadioLogic.Instance.RadioConfig.Elevator_Offset2 = (int)sldElevator_Offset2.Value;
                AircraftRadioLogic.Instance.RadioConfig.Elevator_Reverse1 = (chkElevator_Reverse1.IsChecked ?? false);
                AircraftRadioLogic.Instance.RadioConfig.Elevator_Reverse2 = (chkElevator_Reverse2.IsChecked ?? false);

                AircraftRadioLogic.Instance.RadioConfig.Aileron_Offset1 = (int)sldAileron_Offset1.Value;
                AircraftRadioLogic.Instance.RadioConfig.Aileron_Offset2 = (int)sldAileron_Offset2.Value;
            }
        }

        private void sldElevator_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SaveSettingsFromUI();
        }

        private void chkElevator_Checked(object sender, RoutedEventArgs e)
        {
            SaveSettingsFromUI();
        }

        private void sldAileron_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SaveSettingsFromUI();
        }

        private void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            AircraftRadioLogic.Instance.SaveRadioSettings();
        }

        #endregion

        #region UI

        private void btnComConnect_Click(object sender, RoutedEventArgs e)
        {
            AircraftRadioLogic.Instance.COMConnect();
            AircraftRadioLogic.Instance.StartSendJoystic();
        }

        private void btnComDisconnect_Click(object sender, RoutedEventArgs e)
        {
            AircraftRadioLogic.Instance.StopSendJoystic();
            AircraftRadioLogic.Instance.COMDisconnect();
        }

        private void timerUI_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    SetControllerValues();
                }));
            }
            catch (Exception)
            {
            }
        }

        private void SetControllerValues()
        {
            try
            {
                AircraftState aircraftState = AircraftRadioLogic.Instance.GetCurrentAircraftStateThreadSafe();
                RadioConfigurations radioConfig = AircraftRadioLogic.Instance.RadioConfig;

                if (aircraftState.elevatorValueRaw != this.lastAircraftState.elevatorValueRaw)
                {
                    joyRight.YValue = aircraftState.elevatorValueRaw;

                    plotElevator1.PointX = aircraftState.elevatorValueRaw;
                    plotElevator1.PointY = aircraftState.elevatorValue1;

                    plotElevator2.PointX = aircraftState.elevatorValueRaw;
                    plotElevator2.PointY = aircraftState.elevatorValue2;
                }

                if (aircraftState.aileronValue != this.lastAircraftState.aileronValue)
                {
                    joyRight.XValue = aircraftState.aileronValue - radioConfig.Aileron_MinValue;
                }

                if (aircraftState.rudderValue != this.lastAircraftState.rudderValue)
                {
                    joyLeft.XValue = aircraftState.rudderValue - radioConfig.Rudder_MinValue;
                }

                if (aircraftState.throtleValue != this.lastAircraftState.throtleValue)
                {
                    if (radioConfig.Throtle_ReverseJoystick)
                    {
                        joyLeft.YValue = radioConfig.Throtle_MaxValue -
                            (aircraftState.throtleValue - radioConfig.Throtle_MinValue);
                    }
                    else
                    {
                        joyLeft.YValue = (aircraftState.throtleValue - radioConfig.Throtle_MinValue);
                    }

                }

                this.lastAircraftState = aircraftState;

                this.textBox.Text = Math.Round(AircraftRadioLogic.Instance.GetRadioConnectionFPS(), 1).ToString();

                this.txtTotalSendPackages.Text = AircraftRadioLogic.Instance.GetRadioTotalSends().ToString();

                vtailViewer.LeftValue = (double)aircraftState.elevatorValue1;
                vtailViewer.RightValue = (double)aircraftState.elevatorValue2;

                //if (AircraftRadioLogic.Instance.COMInitialized)
                {
                    txtChan1Value.Text = aircraftState.throtleValue.ToString();
                    txtChan2Value.Text = aircraftState.aileronValue.ToString();
                    txtChan3Value1.Text = aircraftState.elevatorValue1.ToString();
                    txtChan3Value2.Text = aircraftState.elevatorValue2.ToString();
                    txtChan4Value.Text = aircraftState.rudderValue.ToString();
                }
            }
            catch (Exception ex)
            {
                //AddLogItem("ERR", "Error in SetControllerValues" + ex.ToString());
            }

        }

        private void SetUIButtonsLayout()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                btnComConnect.IsEnabled = !AircraftRadioLogic.Instance.COMInitialized;
                btnComDisconnect.IsEnabled = AircraftRadioLogic.Instance.COMInitialized;

            }));
        }

        #endregion

        #region Log

        //private void ShowError(string error)
        //{
        //    AddLogItem("ERR", error);
        //    MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //}

        private void AddLogItem(LogItem logItem)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (listView.Items.Count > 10000)
                    listView.Items.Clear();

                listView.Items.Add(logItem);
                listView.SelectedIndex = listView.Items.Count - 1;

                listView.ScrollIntoView(listView.Items[listView.SelectedIndex]);
            }));

        }

        private void AddConnectionLogItem(string logItem)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (txtConnLog.Text.Length > 1000)
                    txtConnLog.Text = "";
                txtConnLog.Text += logItem;
                txtConnLog.SelectionStart = txtConnLog.Text.Length;
            }));
        }

        #endregion

        private void chkSendChan_Checked(object sender, RoutedEventArgs e)
        {
            List<byte> channelsToSend = new List<byte>();

            if (chkSendChanThrottle.IsChecked ?? false)
                channelsToSend.Add(AircraftRadioLogic.Instance.RadioConfig.Throtle_ChannelCode);
            if (chkSendChanAileron.IsChecked ?? false)
            {
                channelsToSend.Add(AircraftRadioLogic.Instance.RadioConfig.Aileron_ChannelCode1);
                channelsToSend.Add(AircraftRadioLogic.Instance.RadioConfig.Aileron_ChannelCode2);
            }
            if (chkSendChanElevator1.IsChecked ?? false) {
                channelsToSend.Add(AircraftRadioLogic.Instance.RadioConfig.Elevator_ChannelCode1);
            }
            if (chkSendChanElevator2.IsChecked ?? false)
            {
                channelsToSend.Add(AircraftRadioLogic.Instance.RadioConfig.Elevator_ChannelCode2);
            }
            if (chkSendChanRudder.IsChecked ?? false)
            {
                channelsToSend.Add(AircraftRadioLogic.Instance.RadioConfig.Rudder_ChannelCode1);
                channelsToSend.Add(AircraftRadioLogic.Instance.RadioConfig.Rudder_ChannelCode2);
            }

            AircraftRadioLogic.Instance.SetRadioDebugChannelsToSend(channelsToSend.ToArray());
        }
    }

}
