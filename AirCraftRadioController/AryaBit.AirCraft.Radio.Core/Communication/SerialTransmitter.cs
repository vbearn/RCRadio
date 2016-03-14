using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AryaBit.AirCraft.Radio.Core
{

    public class SerialTransmitter
    {

        #region Fields

        private SerialPort port;
        public event Action<string> DateReceived;
        #endregion

        #region init

        public SerialTransmitter(string portToConnect)
        {
            port = new SerialPort(portToConnect, 57600, Parity.None, 8, StopBits.One);
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
            if (DateReceived != null)
            {
                SerialPort sp = (SerialPort)sender;
                string indata = sp.ReadExisting();
                DateReceived(indata);
            }
            

        }

        public void SendData2Bytes(byte[] buffer)
        {
            port.Write(buffer, 0, 3);
        }

        #endregion

        #region List

        public static string[] GetPortNames()
        {
            // Get a list of serial port names.
            string[] ports = SerialPort.GetPortNames();

            return ports;
        }

        #endregion

    }
}
