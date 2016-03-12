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

        private SerialPort port;

        public SerialTransmitter()
        {
            port = new SerialPort("COM5", 57600, Parity.None, 8, StopBits.One);

            port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            port.Open();
        }

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

        public void Close()
        {
            port.Close();
        }


    }
}
