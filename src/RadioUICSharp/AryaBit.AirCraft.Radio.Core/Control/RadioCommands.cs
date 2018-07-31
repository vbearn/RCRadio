using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AryaBit.AirCraft.Radio.Core
{

    public class RadioCommands : IDisposable
    {

        #region Constants

        private const bool DEBUG_MODE = false;
        private byte[] DEBUG_LOG_CHANNELS = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        public byte[] DEBUG_SEND_CHANNELS = new byte[] {  };

        private const byte CHANNEL_RESERVED_BYTES = 9;
        private const byte VALUE_RESERVED_BYTES = 10;
        private const byte COMMAND_SEPARATOR = 244;

        private const float FPS_MAXLIMIT = 20;


        //private const int COM_DELAY = 10;

        #endregion

        #region Fields

        //**** Aircraft State *****
        private AircraftState aircraftState = null;
        private RadioConfigurations config;

        //**** Transmitter *****
        private SerialTransmitter sTransmitter;
        private bool isSendingData = false;

        //**** Performance *****
        public float FPS = 0;
        private DateTime lastFPSTime = DateTime.MinValue;
        private DateTime lastSendTime = DateTime.MinValue;
        private int sendCount = 0;
        public int totalSendCount = 0;

        #endregion

        #region Events

        public event Action<string, string> LogOccured;
        public event Action DisconnectOccured;

        #endregion

        #region init

        public RadioCommands(SerialTransmitter sTransmitter, RadioConfigurations config)
        {
            this.sTransmitter = sTransmitter;
            this.config = config;

            this.isSendingData = true;

        }

        public void Dispose()
        {
            this.isSendingData = false;
        }

        #endregion

        #region Aircraft State

        public void SetAircraftValues(
            AircraftState aircraftState)
        {
            if (this.aircraftState == null)
                this.aircraftState = new AircraftState();

            this.aircraftState.SetValues(aircraftState);
        }

        #endregion

        #region Sending Data

        private void SendingThread()
        {
            this.lastSendTime = DateTime.Now;
            this.lastFPSTime = DateTime.Now;

            try
            {
                while (true)
                {
                    if (!this.isSendingData)
                        return;

                    bool doTransmit = true;

                    // *** Cycle Time ***
                    TimeSpan pastCycleTime = DateTime.Now - this.lastSendTime;
                    if (pastCycleTime.TotalMilliseconds < ((float)1000 / FPS_MAXLIMIT))
                    {
                        //Thread.Sleep(200);
                        doTransmit = false;
                    }

                    #region Transmit

                    if (doTransmit)
                    {
                        this.totalSendCount++;
                        this.sendCount++;
                        this.lastSendTime = DateTime.Now;
                        //Console.WriteLine(pastTime.TotalMilliseconds + " #" + this.sendCount);

                        this.sTransmitter.StartBuffer();

                        AddBufferChannelData(this.config.Throtle_ChannelCode, this.aircraftState.throtleValue);

                        // *** Aileron ***
                        if (this.config.AileronControl == AileronMode.SingleMode)
                        {
                            AddBufferChannelData(this.config.Aileron_ChannelCode1, this.aircraftState.aileronValue);
                            AddBufferChannelData(this.config.Aileron_ChannelCode2, 0);
                        }
                        else
                        {
                            AddBufferChannelData(this.config.Aileron_ChannelCode1, (byte)(this.aircraftState.aileronValue + this.config.Aileron_Offset1));
                            AddBufferChannelData(this.config.Aileron_ChannelCode2, (byte)(this.aircraftState.aileronValue + this.config.Aileron_Offset2));
                            //AddBufferChannelData(this.config.Aileron_ChannelCode2,
                            //   (byte)(this.config.Aileron_MaxValue - (this.aircraftState.aileronValue - this.config.Aileron_MinValue)));
                        }
                        // ****************

                        // *** Elevator ***
                        if (this.config.ElevatorControl == ElevatorMode.SingleMode)
                        {
                            AddBufferChannelData(this.config.Elevator_ChannelCode1, this.aircraftState.elevatorValue1);
                            AddBufferChannelData(this.config.Elevator_ChannelCode2, 0);
                        }
                        else
                        {
                            AddBufferChannelData(this.config.Elevator_ChannelCode1, this.aircraftState.elevatorValue1);
                            AddBufferChannelData(this.config.Elevator_ChannelCode2, this.aircraftState.elevatorValue2);

                            //byte elevChan2Val = (byte)(this.aircraftState.elevatorValue + this.config.Elevator_Offset2);
                            //if (this.config.Elevator_ReverseLeftRight)
                            //    elevChan2Val = (byte)(this.config.Elevator_MaxValue - (elevChan2Val - this.config.Elevator_MinValue));
                        }
                        // **************

                        // *** Rudder ***
                        if (this.config.RudderControl == RudderMode.SingleMode)
                        {
                            AddBufferChannelData(this.config.Rudder_ChannelCode1, this.aircraftState.rudderValue);
                            AddBufferChannelData(this.config.Rudder_ChannelCode2, 0);
                        }
                        else
                        {
                            AddBufferChannelData(this.config.Rudder_ChannelCode1, this.aircraftState.rudderValue);
                            AddBufferChannelData(this.config.Rudder_ChannelCode2, this.aircraftState.rudderValue);
                        }
                        // *************


                        for (int tempChannelIndex = 8; tempChannelIndex <= 10; tempChannelIndex++)
                        {
                            AddBufferChannelData((byte)tempChannelIndex, 0);
                        }

                        this.sTransmitter.SendBuffer();
                    }

                    #endregion


                    // *** FPS ***
                    TimeSpan pastFPSTime = DateTime.Now - this.lastFPSTime;
                    if (pastFPSTime.TotalMilliseconds > 1000)
                    {
                        this.lastFPSTime = DateTime.Now;
                        this.FPS = (float)this.sendCount / (float)(pastFPSTime.TotalMilliseconds / (double)1000);
                        this.sendCount = 0;
                    }


                    Thread.Sleep(30);

                }
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException)
                {
                    if (DisconnectOccured != null)
                        DisconnectOccured();
                }

            }
        }

        public void StartSendingThread()
        {
            this.isSendingData = true;
            new Thread(() =>
            {
                SendingThread();
            }).Start();
        }

        public void StopSendingThread()
        {
            this.isSendingData = false;
        }

        public void AddBufferChannelData(byte channelNumber, byte value)
        {
            if (DEBUG_MODE)
            {
                if (!DEBUG_SEND_CHANNELS.Contains(channelNumber))
                    return;
            }

            if (channelNumber + CHANNEL_RESERVED_BYTES > 255 || channelNumber < 1)
                throw new ArgumentException("Invalid Channel Number");

            if (value + VALUE_RESERVED_BYTES > 255 || value < 0)
                throw new ArgumentException("Invalid Value");

            byte convertedChannelNumber = (byte)(channelNumber + CHANNEL_RESERVED_BYTES);
            byte convertedValue = (byte)(value + VALUE_RESERVED_BYTES);

            byte[] commandBuffer = new byte[3] { COMMAND_SEPARATOR, convertedChannelNumber, convertedValue };

            this.sTransmitter.AddBuffer(commandBuffer);

            if (DEBUG_MODE)
            {
                if (LogOccured != null)
                {
                    if (DEBUG_LOG_CHANNELS.Contains(channelNumber))
                        LogOccured(channelNumber.ToString(), value.ToString());
                }

            }

        }

        #endregion

    }

    public class AircraftState
    {
        #region Fields

        //**** Aircraft State Values *****
        public byte rudderValue = 0;
        public byte aileronValue = 0;
        public int elevatorValueRaw = 0;
        public byte elevatorValue1 = 0;
        public byte elevatorValue2 = 0;
        public byte throtleValue = 0;

        #endregion

        #region init

        public AircraftState() { }

        //public AircraftState(byte rudderValue, byte aileronValue, byte elevatorValue1, byte elevatorValue2, byte throtleValue)
        //{
        //    SetValues(new AircraftState()
        //    {
        //        rudderValue = rudderValue,
        //        aileronValue = aileronValue,
        //        elevatorValue1 = elevatorValue1,
        //        elevatorValue2 = elevatorValue2,
        //        throtleValue = throtleValue
        //    });
        //}


        #endregion

        #region Value

        public void SetValues(AircraftState aircraftState)
        {
            this.rudderValue = aircraftState.rudderValue;
            this.aileronValue = aircraftState.aileronValue;
            this.elevatorValueRaw = aircraftState.elevatorValueRaw;
            this.elevatorValue1 = aircraftState.elevatorValue1;
            this.elevatorValue2 = aircraftState.elevatorValue2;
            this.throtleValue = aircraftState.throtleValue;
        }

        public AircraftState Clone()
        {
            AircraftState clone = new AircraftState();
            clone.SetValues(this);
            return clone;
        }


        #endregion

    }

}
