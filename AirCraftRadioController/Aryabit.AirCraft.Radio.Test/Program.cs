using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AryaBit.AirCraft.Radio.Test
{
    class Program
    {

        private const byte COMMAND_SEPARATOR = 244;

        private static DateTime lastSendTime = DateTime.MinValue;

        static Semaphore sem = new Semaphore(0, 1);
        //static object semObj = new object();
        static void Main(string[] args)
        {
            AryaBit.AirCraft.Radio.Core.SerialTransmitter sTransmitter = new AryaBit.AirCraft.Radio.Core.SerialTransmitter("COM6");
            sTransmitter.DateReceived += STransmitter_DateReceived;
            sTransmitter.Open();

            Console.ReadLine();

            for (int j = 0; j < 100; j++)
            {
                sTransmitter.StartBuffer();
                for (int i = 0; i < 10; i++)
                {
                    sTransmitter.AddBuffer(new byte[3] { (byte)(244), (byte)(10 + i), (byte)(100 + i) });
                }
                Console.WriteLine("Sending Buffer...");
                lastSendTime = DateTime.Now;
                sTransmitter.SendBuffer();

                //sem.WaitOne();
                Thread.Sleep(30);
                //break;
            }

                Console.WriteLine("Sending finished.");
            Console.ReadLine();
        }

        private static void STransmitter_DateReceived(string obj)
        {
            TimeSpan span = DateTime.Now - lastSendTime;
            Console.WriteLine("Round Trip Time Ms: " + span.TotalMilliseconds);
            //sem.Release();
        }

    }
}
