using AryaBit.AirCraft.Radio.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AryaBit.AirCraft.Radio.Core
{
    public class AircraftRadioLogic
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
        private RadioConfigurations radioConfig;
        public RadioConfigurations RadioConfig
        {
            get { return this.radioConfig; }
        }

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
            AddSysLogItem("App Started.");

            this.currentAircraftState = new AircraftState()
            {
                aileronValue = 90,
                rudderValue = 90,
                throtleValue = 90,
                elevatorValue1 = 90,
                elevatorValue2 = 90,
                elevatorValueRaw = 90,
            };

            AddSysLogItem("Initializing modules...");

            LoadRadioSettings();

            //COMConnect();

            try
            {
                AddSysLogItem("Initializing Joystick module...");
                initJoystic();

            }
            catch (Exception)
            {
                AddErrorLogItem("Joystick module initialization FAILED.");
            }

            AddSysLogItem("All Modules initialized.");

        }

        #endregion

        #region Radio Settings

        private void LoadRadioSettings()
        {
            this.radioConfig = null;
            if (!string.IsNullOrEmpty(Settings.Default.RadioConfig))
            {
                this.radioConfig = RadioConfigurations.DeSerializeObject(Settings.Default.RadioConfig);
            }

            if (this.radioConfig == null)
            {
                this.radioConfig = new RadioConfigurations()
                {

                    Throtle_MinValue = 0,
                    Throtle_MaxValue = 180,
                    Throtle_ChannelCode = 1,
                    Throtle_ReverseJoystick = true,

                    AileronControl = AileronMode.DualMode,
                    Aileron_MinValue = 45,
                    Aileron_MaxValue = 135,
                    Aileron_ChannelCode1 = 2,
                    Aileron_ChannelCode2 = 3,
                    Aileron_ReverseJoystick = false,
                    Aileron_Offset1 = -5,
                    Aileron_Offset2 = +32,

                    ElevatorControl = ElevatorMode.DualMode,
                    Elevator_MinValue1 = 97,
                    Elevator_MaxValue1 = 150,
                    Elevator_MinValue2 = 81, // right elev up
                    Elevator_MaxValue2 = 126, // right elev down
                    Elevator_ChannelCode1 = 4,
                    Elevator_ChannelCode2 = 5,
                    Elevator_Reverse1 = false,
                    Elevator_Reverse2 = true,
                    Elevator_Offset1 = -16,
                    Elevator_Offset2 = +25,
                    Elevator_Curve1 = CurveMode.SymmetricDegree3,
                    Elevator_Curve2 = CurveMode.SymmetricDegree3,

                    RudderControl = RudderMode.SingleMode,
                    Rudder_MinValue = 45,
                    Rudder_MaxValue = 135,
                    Rudder_ChannelCode1 = 6,
                    Rudder_ChannelCode2 = 7,
                    Rudder_ReverseJoystick = false,

                };
            }
        }

        public void SaveRadioSettings()
        {
            Settings.Default.RadioConfig = this.radioConfig.SerializeObject();
            Settings.Default.Save();
        }


        #endregion

        #region Radio

        public void COMConnect()
        {
            try
            {
                string[] ports = SerialTransmitter.GetPortNames();
                string portToConnect;

                if (ports.Length > 0)
                {
                    portToConnect = ports[0];
                }
                else
                {
                    AddErrorLogItem("COM connect failed. You should have 1 COM port!");
                    return;
                }

                AddSysLogItem("Connecting to " + portToConnect + "...");

                this.comTransmitter = new SerialTransmitter(portToConnect);
                this.comTransmitter.DateReceived += ComTransmitter_DateReceived;
                this.comTransmitter.Open();

                AddSysLogItem("Connected to " + portToConnect + ".");


            }
            catch (Exception ex)
            {
                this.rCommand = null;
                AddErrorLogItem("COM connection failed.");
                return;
            }

            this.rCommand = new Core.RadioCommands(comTransmitter, this.radioConfig);
            this.rCommand.LogOccured += RCommand_LogOccured;
            this.rCommand.DisconnectOccured += () =>
            {
                AddSysLogItem("COM Error / Force disconnect.");
                COMDisconnect();
            };
            if (this.currentAircraftState != null)
                this.rCommand.SetAircraftValues(this.currentAircraftState);
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
                AddErrorLogItem("COM disconnect failed. Trying to dispose RadioCommand.");
            }

            try
            {
                this.rCommand.Dispose();
            }
            catch (Exception ex) { }

            this.rCommand = null;
            RaiseNeedUpdateUI();

            AddSysLogItem("COM disconnected.");

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


        public float GetRadioTotalSends()
        {
            if (this.comInitialized)
                return this.rCommand.totalSendCount;
            else
                return 0;
        }


        public void SetRadioDebugChannelsToSend(byte[] channelsToSend)
        {
            if (this.comInitialized)
                this.rCommand.DEBUG_SEND_CHANNELS = channelsToSend;
        }

        #endregion

        #region Joystick

        public void initJoystic()
        {
            var directInputList = new List<SlimDX.DirectInput.DeviceInstance>();
            directInputList.AddRange(Joystick.GetControllers());
            Guid deviceToConnect;
            string deviceToConnectName;

            if (directInputList.Count > 0)
            {
                deviceToConnectName = directInputList[1].InstanceName;
                deviceToConnect = directInputList[1].InstanceGuid;
            }
            else
            {
                AddErrorLogItem("Joystick connection failed. You should have at least 1 controller port!");
                return;
            }

            AddSysLogItem("Connecting to contoller " + deviceToConnectName + "...");
            this.joystick = new Joystick(deviceToConnect);
            this.joystick.OnStateUpdated += Joystick_OnStateUpdated;
            this.joystick.StartCaptureThread();
            this.joystickInitialized = true;
            AddSysLogItem("Connected to contoller " + deviceToConnectName + ".");
        }

        public void StartSendJoystic()
        {
            if (!this.joystickInitialized)
            {
                AddErrorLogItem("Joystick has not been initialized.");
                return;
            }

            if (!this.comInitialized)
            {
                AddErrorLogItem("Connection has not been initialized.");
                return;
            }

            AddSysLogItem("Starting to sending joystick values...");

            this.joystickSending = true;
            this.rCommand.StartSendingThread();

            RaiseNeedUpdateUI();
        }

        private byte MapJoystickPosInRange(int joystickValue, CurveMode curveMode, byte rangeMin, byte rangeMax, int offset, bool reverse = false)
        {
            byte mappedValue = (byte)ChannelCurves.DoChannelCurve(
                joystickValue, Joystick.JOYSTICK_MAXVALUE, curveMode, rangeMin, rangeMax);
            //byte mappedValue = (byte)(((float)(joystickValue * (rangeMax - rangeMin)) / ) + rangeMin);

            mappedValue = (byte)(mappedValue + offset);
            if (reverse)
                mappedValue = (byte)(rangeMax - (mappedValue - rangeMin));
            return mappedValue;
        }

        private void Joystick_OnStateUpdated(SlimDX.DirectInput.JoystickState state)
        {

            if (!this.joystickInitialized)
                return;

            var aircraftState = new AircraftState()
            {
                rudderValue = MapJoystickPosInRange(
                    state.X, CurveMode.Linear, this.radioConfig.Rudder_MinValue,  this.radioConfig.Rudder_MaxValue, 0, this.radioConfig.Rudder_ReverseJoystick),
                aileronValue = MapJoystickPosInRange(
                    state.RotationX, CurveMode.Linear, this.radioConfig.Aileron_MinValue, this.radioConfig.Aileron_MaxValue, 0, this.radioConfig.Aileron_ReverseJoystick),
                elevatorValueRaw = state.RotationY,
                elevatorValue1 = MapJoystickPosInRange(
                    state.RotationY, this.radioConfig.Elevator_Curve1, this.radioConfig.Elevator_MinValue1,
                    this.radioConfig.Elevator_MaxValue1,
                    this.radioConfig.Elevator_Offset1,
                    this.radioConfig.Elevator_Reverse1),
                elevatorValue2 = MapJoystickPosInRange(
                    state.RotationY, this.radioConfig.Elevator_Curve2, this.radioConfig.Elevator_MinValue2,
                    this.radioConfig.Elevator_MaxValue2,
                    this.radioConfig.Elevator_Offset2,
                    this.radioConfig.Elevator_Reverse2),
                throtleValue = MapJoystickPosInRange(
                    state.Y, CurveMode.Linear, this.radioConfig.Throtle_MinValue, this.radioConfig.Throtle_MaxValue, 0, this.radioConfig.Throtle_ReverseJoystick),
            };

            //if (this.joystickSending)
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

            AddSysLogItem("Stopped sending joystick values.");
        }

        #endregion

        #region Log

        private void AddSysLogItem(string message)
        {
            if (LogOccured != null)
                LogOccured(new LogItem() { Time = DateTime.Now.ToString(), Type = "SYS", Message = message });
        }
        private void AddErrorLogItem(string message)
        {
            if (LogOccured != null)
                LogOccured(new LogItem() { Time = DateTime.Now.ToString(), Type = "ERR", Message = message });
        }

        private void AddLogItem(string type, string message)
        {
            if (LogOccured != null)
                LogOccured(new LogItem() { Time = DateTime.Now.ToString(), Type = type, Message = message });
        }

        #endregion

    }

    public class LogItem
    {
        public string Time { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
    }

}
