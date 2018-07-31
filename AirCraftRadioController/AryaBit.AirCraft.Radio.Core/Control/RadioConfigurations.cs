using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AryaBit.AirCraft.Radio.Core
{
    public class RadioConfigurations
    {
        public RudderMode RudderControl { get; set; }
        public ElevatorMode ElevatorControl { get; set; }
        public AileronMode AileronControl { get; set; }


        public byte Throtle_MinValue { get; set; }
        public byte Throtle_MaxValue { get; set; }
        public byte Throtle_ChannelCode { get; set; } //= 1;
        public bool Throtle_ReverseJoystick { get; set; }



        public byte Aileron_MinValue { get; set; }
        public byte Aileron_MaxValue { get; set; }
        public byte Aileron_ChannelCode1 { get; set; } //= 2;
        public byte Aileron_ChannelCode2 { get; set; } //= 3;
        public int Aileron_Offset1 { get; set; }
        public int Aileron_Offset2 { get; set; }
        public bool Aileron_ReverseJoystick { get; set; }
        


        public byte Elevator_MinValue1 { get; set; }
        public byte Elevator_MaxValue1 { get; set; }
        public byte Elevator_MinValue2 { get; set; }
        public byte Elevator_MaxValue2 { get; set; }
        public byte Elevator_ChannelCode1 { get; set; }// = 4;
        public byte Elevator_ChannelCode2 { get; set; } //= 5;
        public int Elevator_Offset1 { get; set; }
        public int Elevator_Offset2 { get; set; }
        public bool Elevator_Reverse1 { get; set; }
        public bool Elevator_Reverse2 { get; set; }
        public CurveMode Elevator_Curve1 { get; set; }
        public CurveMode Elevator_Curve2 { get; set; }



        public byte Rudder_MinValue { get; set; }
        public byte Rudder_MaxValue { get; set; }
        public byte Rudder_ChannelCode1 { get; set; }// 6;
        public byte Rudder_ChannelCode2 { get; set; }// = 7;
        public bool Rudder_ReverseJoystick { get; set; }


        public static RadioConfigurations DeSerializeObject(string serializedXml)
        {
            RadioConfigurations obj;

            XmlSerializer serializer = new XmlSerializer(typeof(RadioConfigurations));

            using (StringReader textReader = new StringReader(serializedXml))
            {
                obj = (RadioConfigurations)serializer.Deserialize(textReader);
            }

            return obj;
        }

        public string SerializeObject()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(this.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, this);
                return textWriter.ToString();
            }
        }
    }

    public enum RudderMode
    {
        SingleMode,
        DualMode
    }

    public enum ElevatorMode
    {
        SingleMode,
        DualMode
    }

    public enum AileronMode
    {
        SingleMode,
        DualMode
    }


    public enum CurveMode
    {
        Linear,
        SymmetricDegree3
    }
}
