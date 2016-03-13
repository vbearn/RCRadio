using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AryaBit.AirCraft.Radio.Core
{

    class SerialTransmitter
    {

        #region Fields

        private SerialPort port;

        #endregion

        #region init

        public SerialTransmitter()
        {
            port = new SerialPort("COM5", 57600, Parity.None, 8, StopBits.One);

            port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }

        public void Open()
        {
            port.Open();
        }

        public void Close()
        {
            port.Close();
        }

        #endregion

        #region Send Receive

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            System.Diagnostics.Debug.WriteLine("Data Received:" + indata);

        }

        public void SendData2Bytes(byte[] buffer)
        {
            port.Write(buffer, 0, 3);
        }

        #endregion

    }
}
