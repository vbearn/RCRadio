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
        }

        private void initControlsDefaults()
        {
            var dragExculdedUIElements = new UIElement[] {
                
            };

            DragHandler.DragMoveWindow(this, null,
                () =>
                {
                    //DisableFollowCursorOnMove();
                    this.windowIsMoving = true;
                },
                (hasMoved) =>
                {
                    this.windowIsMoving = false;
                    this.windowDragEndTime = DateTime.Now;
                },
                dragExculdedUIElements);


            this.lastAircraftState = new AircraftState(0, 0, 0, 0);

            joyLeft.XValue = 90;
            joyLeft.YValue = 90;
            joyRight.XValue = 90;
            joyRight.YValue = 90;

            timerUI = new System.Timers.Timer();
            timerUI.Interval = 100;
            timerUI.Elapsed += timerUI_Elapsed;
            timerUI.Start();

        }

        #endregion

        #region UI

        private void btnComConnect_Click(object sender, RoutedEventArgs e)
        {
            AircraftRadioLogic.Instance.COMConnect();
        }

        private void btnComDisconnect_Click(object sender, RoutedEventArgs e)
        {
            AircraftRadioLogic.Instance.COMDisconnect();
        }

        private void btnStartSendJoystic_Click(object sender, RoutedEventArgs e)
        {
            AircraftRadioLogic.Instance.StartSendJoystic();
        }

        private void btnStopSendJoystic_Click(object sender, RoutedEventArgs e)
        {
            AircraftRadioLogic.Instance.StopSendJoystic();
        }

        private void timerUI_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                SetControllerValues();
            }));
        }

        private void SetControllerValues()
        {
            try
            {
                AircraftState aircraftState = AircraftRadioLogic.Instance.GetCurrentAircraftStateThreadSafe();

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

                this.textBox.Text = AircraftRadioLogic.Instance.GetRadioConnectionFPS().ToString();
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

               btnStartSendJoystic.IsEnabled = !AircraftRadioLogic.Instance.JoystickSending;
               btnStopSendJoystic.IsEnabled = AircraftRadioLogic.Instance.JoystickSending;
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
                txtConnLog.Text += logItem;
            }));
        }

        #endregion

        #region Test

        private void btnTestChan1_Click(object sender, RoutedEventArgs e)
        {
            AircraftRadioLogic.Instance.SendChannelTestData(1);
        }

        private void btnTestChan2_Click(object sender, RoutedEventArgs e)
        {
            AircraftRadioLogic.Instance.SendChannelTestData(2);
        }

        private void btnTestChan3_Click(object sender, RoutedEventArgs e)
        {
            AircraftRadioLogic.Instance.SendChannelTestData(3);
        }

        private void btnTestChan4_Click(object sender, RoutedEventArgs e)
        {
            AircraftRadioLogic.Instance.SendChannelTestData(4);
        }

        #endregion

    }

}
