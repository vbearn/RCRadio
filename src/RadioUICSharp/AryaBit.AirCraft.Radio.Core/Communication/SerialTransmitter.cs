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

        private List<byte> bufferToWrite;

        #endregion

        #region init

        public SerialTransmitter(string portToConnect)
        {
            port = new SerialPort(portToConnect, 115200, Parity.None, 8, StopBits.One);
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
            {
                SerialPort sp = (SerialPort)sender;
                string indata = sp.ReadExisting();
                //Console.WriteLine(indata);
                foreach (var item in indata)
                    Console.Write((int)((byte)item) + ", ");
                Console.WriteLine("");
                if (DateReceived != null)
                    DateReceived(indata);
            }


        }


        public void StartBuffer()
        {
            this.bufferToWrite = new List<byte>();
        }
        public void AddBuffer(byte[] buffer)
        {
            this.bufferToWrite.AddRange(buffer);
        }


        public void SendBuffer()
        {
            port.Write(this.bufferToWrite.ToArray(), 0, bufferToWrite.Count);

            //Console.WriteLine((int)buffer[1] + "," + (int)buffer[2]);
            //System.Threading.Thread.Sleep(100);
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
