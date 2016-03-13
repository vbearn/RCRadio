using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AryaBit.AirCraft.Radio.Core
{

    public class RadioCommands
    {

        #region Constants

        private const byte CHANNEL_RESERVED_BYTES = 9;
        private const byte VALUE_RESERVED_BYTES = 10;
        private const byte COMMAND_SEPARATOR = 244;

        public const byte RUDDER_MINVALUE = 0;
        public const byte RUDDER_MAXVALUE = 180;
        public const byte RUDDER_CHANNELCODE = 1;

        public const byte AILERON_MINVALUE = 0;
        public const byte AILERON_MAXVALUE = 180;
        public const byte AILERON_CHANNELCODE = 2;

        public const byte ELEVATOR_MINVALUE = 0;
        public const byte ELEVATOR_MAXVALUE = 180;
        public const byte ELEVATOR_CHANNELCODE = 3;

        public const byte THROTLE_MINVALUE = 0;
        public const byte THROTLE_MAXVALUE = 180;
        public const byte THROTLE_CHANNELCODE = 4;

        #endregion

        #region Fields 

        //**** Aircraft State *****
        AircraftState aircraftState;

        //**** Transmitter *****
        private SerialTransmitter sTransmitter;
        private bool isSendingData = false;

        //**** Performance *****
        public float FPS = 0;
        private DateTime lastSendTime = DateTime.MinValue;
        private int sendCount = 0;

        public event Action<string, string> LogOccured;

        #endregion

        #region init

        public RadioCommands()
        {
            this.isSendingData = true;

            this.aircraftState = new AircraftState(
                RUDDER_MINVALUE, AILERON_MINVALUE, ELEVATOR_MINVALUE, THROTLE_MINVALUE);
        }

        public void initComPort()
        {
            this.sTransmitter = new SerialTransmitter();
        }

        public void CloseComPort()
        {
            this.sTransmitter.Close();
        }

        #endregion

        #region Aircraft State

        public void SetAircraftValues(
            AircraftState aircraftState)
        {
            this.aircraftState.SetValues(aircraftState);
        }

        #endregion

        #region Sending Data

        private const int comDelay = 200;
        private void SendingThread()
        {
            this.lastSendTime = DateTime.Now;

            while (true)
            {
                if (this.isSendingData)
                {
                    SendChannelData(RUDDER_CHANNELCODE, this.aircraftState.rudderValue);
                    SendChannelData(AILERON_CHANNELCODE, this.aircraftState.aileronValue);
                    SendChannelData(ELEVATOR_CHANNELCODE, this.aircraftState.elevatorValue);
                    SendChannelData(THROTLE_CHANNELCODE, this.aircraftState.throtleValue);
                    Thread.Sleep(comDelay);

                    this.sendCount++;

                    TimeSpan pastTime = DateTime.Now - this.lastSendTime;
                    if (pastTime.TotalMilliseconds > 1000)
                    {
                        this.lastSendTime = DateTime.Now;
                        this.FPS = (float)this.sendCount / (float)(pastTime.TotalMilliseconds / (double)1000);
                        this.sendCount = 0;
                    }

                    Thread.Sleep(comDelay);
                }
                else
                {
                    Thread.Sleep(20);
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

        public void SendChannelData(byte channelNumber, byte value)
        {

            if (channelNumber + CHANNEL_RESERVED_BYTES > 255 || channelNumber < 1)
                throw new ArgumentException("Invalid Channel Number");

            if (value + VALUE_RESERVED_BYTES > 255 || value < 0)
                throw new ArgumentException("Invalid Value");

            byte convertedChannelNumber = (byte)(channelNumber + CHANNEL_RESERVED_BYTES);
            byte convertedValue = (byte)(value + VALUE_RESERVED_BYTES);

            byte[] commandBuffer = new byte[3] { COMMAND_SEPARATOR, convertedChannelNumber, convertedValue };

            this.sTransmitter.SendData2Bytes(commandBuffer);

            if (LogOccured != null)
                LogOccured(channelNumber.ToString(), value.ToString());


        }

        #endregion

    }

    public class AircraftState
    {
        #region Fields 

        //**** Aircraft State Values *****
        public byte rudderValue = 0;
        public byte aileronValue = 0;
        public byte elevatorValue = 0;
        public byte throtleValue = 0;

        #endregion

        #region init

        public AircraftState() { }

        public AircraftState(byte rudderValue, byte aileronValue, byte elevatorValue, byte throtleValue)
        {
            SetValues(new AircraftState()
            {
                rudderValue = rudderValue,
                aileronValue = aileronValue,
                elevatorValue = elevatorValue,
                throtleValue = throtleValue
            });
        }


        #endregion

        #region Value

        public void SetValues(
            AircraftState aircraftState)
        {
            this.rudderValue = aircraftState.rudderValue;
            this.aileronValue = aircraftState.aileronValue;
            this.elevatorValue = aircraftState.elevatorValue;
            this.throtleValue = aircraftState.throtleValue;
        }

        #endregion

    }

}
