using AryaBit.AirCraft.Radio.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AryaBit.AirCraft.Radio.UI
{
    class AircraftRadioLogic
    {

        #region Instance

        private static AircraftRadioLogic instance;
        public static AircraftRadioLogic Instance
        {
            get
            {
                if (instance == null)
                    instance = new AircraftRadioLogic();

                return instance;
            }
        }

        #endregion

        #region Fields

        //**** Radio ****
        private Radio.Core.RadioCommands rCommand;
        private SerialTransmitter comTransmitter;
        private bool comInitialized = false;
        public bool COMInitialized
        {
            get { return this.comInitialized; }
        }

        //**** Jotsttick ****
        private Joystick joystick;
        private bool joystickInitialized = false;
        private bool joystickSending = false;
        public bool JoystickSending
        {
            get { return this.joystickSending; }
        }

        //**** AircraftState ****
        private object currentAircraftStateLock = new object();
        private AircraftState currentAircraftState;
        public AircraftState GetCurrentAircraftStateThreadSafe()
        {
            AircraftState aircraftState;

            lock (currentAircraftStateLock)
            {
                aircraftState = this.currentAircraftState.Clone();
            }
            return aircraftState;
        }

        #endregion

        #region Events

        public event Action<LogItem> LogOccured;
        public event Action<string> ConnectionLogOccured;
        public event Action NeedUpdateUI;
        private void RaiseNeedUpdateUI()
        {
            if (NeedUpdateUI != null)
                NeedUpdateUI();
        }

        #endregion

        #region init

        public void initModules()
        {
            AddLogItem("SYS", "App Started.");

            this.currentAircraftState = new AircraftState(90, 90, 90, 90);

            AddLogItem("SYS", "Initializing modules...");

            COMConnect();

            try
            {
                AddLogItem("SYS", "Initializing Joystick module...");
                initJoystic();
                
            }
            catch (Exception)
            {
                AddLogItem("ERR", "Joystick module initialization FAILED.");
            }

            AddLogItem("SYS", "All Modules initialized.");

        }

        #endregion

        #region Radio

        public void COMConnect()
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
                    AddLogItem("ERR", "COM connect failed. You should have only 1 COM port!");
                    return;
                }

                AddLogItem("SYS", "Connecting to " + portToConnect + "...");

                this.comTransmitter = new SerialTransmitter(portToConnect);
                this.comTransmitter.DateReceived += ComTransmitter_DateReceived;
                this.comTransmitter.Open();

                AddLogItem("SYS", "Connected to " + portToConnect + ".");


            }
            catch (Exception ex)
            {
                this.rCommand = null;
                AddLogItem("ERR", "COM connection failed.");
                return;
            }

            this.rCommand = new Core.RadioCommands(comTransmitter);
            this.rCommand.LogOccured += RCommand_LogOccured;
            this.rCommand.StartSendingThread();

            this.comInitialized = true;
            RaiseNeedUpdateUI();
        }

        private void ComTransmitter_DateReceived(string connLog)
        {
            if (ConnectionLogOccured != null)
                ConnectionLogOccured(connLog);
        }

        public void COMDisconnect()
        {
            this.comInitialized = false;

            try
            {
                this.comTransmitter.Close();
            }
            catch (Exception ex)
            {
                AddLogItem("ERR", "COM disconnect failed. Trying to dispose RadioCommand.");
            }

            try
            {
                this.rCommand.Dispose();
            }
            catch (Exception ex) { }

            this.rCommand = null;
            RaiseNeedUpdateUI();
        }

        private void RCommand_LogOccured(string chan, string value)
        {
            AddLogItem(chan, value);
        }

        public float GetRadioConnectionFPS()
        {
            if (this.comInitialized)
                return this.rCommand.FPS;
            else
                return 0;
        }

        #endregion

        #region Joystick

        public void initJoystic()
        {
            var directInputList = new List<SlimDX.DirectInput.DeviceInstance>();
            directInputList.AddRange(Joystick.GetControllers());
            Guid deviceToConnect;
            string deviceToConnectName;

            if (directInputList.Count == 1)
            {
                deviceToConnectName = directInputList[0].InstanceName;
                deviceToConnect = directInputList[0].InstanceGuid;
            }
            else
            {
                AddLogItem("ERR", "Joystick connection failed. You should have only 1 controller port!");
                return;
            }

            AddLogItem("SYS", "Connecting to contoller " + deviceToConnectName + "...");
            this.joystick = new Joystick(deviceToConnect);
            this.joystick.OnStateUpdated += Joystick_OnStateUpdated;
            this.joystick.StartCaptureThread();
            this.joystickInitialized = true;
            AddLogItem("SYS", "Connected to contoller " + deviceToConnectName + ".");
        }

        public void StartSendJoystic()
        {
            if (!this.joystickInitialized)
            {
                AddLogItem("ERR", "Joystick has not been initialized.");
                return;
            }

            if (!this.comInitialized)
            {
                AddLogItem("ERR", "Connection has not been initialized.");
                return;
            }

            AddLogItem("SYS", "Starting to sending joystick values...");

            this.joystickSending = true;

            RaiseNeedUpdateUI();
        }

        private void Joystick_OnStateUpdated(SlimDX.DirectInput.JoystickState state)
        {

            if (!this.joystickInitialized)
                return;

            var aircraftState = new AircraftState()
            {
                rudderValue = (byte)(state.X * 180 / 65533),
                aileronValue = (byte)(state.RotationX * 180 / 65533),
                elevatorValue = (byte)(state.Y * 180 / 65533),
                throtleValue = (byte)(state.RotationY * 180 / 65533),
            };

            if (this.joystickSending)
            {
                if (this.comInitialized)
                    this.rCommand.SetAircraftValues(aircraftState);
            }

            lock (currentAircraftStateLock)
            {
                this.currentAircraftState = aircraftState;
            }

        }

        public void StopSendJoystic()
        {
            if (this.comInitialized)
                this.rCommand.StopSendingThread();

            this.joystickSending = false;

            RaiseNeedUpdateUI();

            AddLogItem("SYS", "Stopped sending joystick values.");
        }

        #endregion

        #region Test

        public void SendChannelTestData(byte channelNumber)
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

        #region Log

        private void AddLogItem(string channel, string value)
        {
            if (LogOccured != null)
                LogOccured(new LogItem() { Time = DateTime.Now.ToString(), Channel = channel, Value = value });
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
