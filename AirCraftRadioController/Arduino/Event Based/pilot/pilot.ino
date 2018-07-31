
#include <SPI.h>
#include <Mirf.h>
#include <nRF24L01.h>
#include <MirfHardwareSpiDriver.h>
#include <Servo.h>

const byte commandChar = 244;
const byte MAXCHANNELS = 10;
const byte RES_FACTOR = 1;      // Factor of noise resistance
const byte NOISE_THR = 30;      // threshold of noise detection
const float LERPFACTOR = 0.3;
const float ESCLERPFACTOR = 0.2;

// M_M
const byte DEBUG_LEVEL_NONE = 0;
const byte DEBUG_LEVEL_INFO = 1;
const byte DEBUG_LEVEL_VERBOSE = 2;

const byte CUR_DEBUG_LEVEL = 0;

const boolean LERPENABLED = false;
// ***

Servo servo_1, servo_2, servo_3, servo_4 , servo_5, ESC;

byte lasts[MAXCHANNELS];
byte counters[MAXCHANNELS];
void setup()
{
  Serial.begin(9600);
  Mirf.spi = &MirfHardwareSpi;
  Mirf.init();
  Mirf.setTADDR((byte *)"remot");
  Mirf.setRADDR((byte *)"pilot");

  Mirf.payload = 32;

  Mirf.config();
  delay(1);

  // M_M
  if (CUR_DEBUG_LEVEL >= DEBUG_LEVEL_INFO)
    Serial.println("Listening...");

  ESC.attach(9);
  servo_1.attach(2);
  servo_2.attach(3);
  servo_3.attach(4);
  servo_4.attach(5);
  servo_5.attach(6);
  Mirf.send((byte*)"Pilot Ready");
}
int msgCounter = 0;

void loop()
{
  // A_A
      if (CUR_DEBUG_LEVEL >= DEBUG_LEVEL_VERBOSE)
        Serial.println("Im Alive");
      // ***
  if (Mirf.dataReady())
  {
    // M_M
      if (CUR_DEBUG_LEVEL >= DEBUG_LEVEL_VERBOSE)
        Serial.println("Data Ready");
      // ***
    if (!Mirf.isSending())
    {
      byte data[32];
      //Serial.println("msg ready");
      Mirf.getData(data);

      parseCommand(data);

      // M_M
      if (CUR_DEBUG_LEVEL >= DEBUG_LEVEL_VERBOSE)
      {
        Serial.print("msg parsed");
        Serial.println((char*) data);
      }
      // ***

      //String msgBuffer = String ((char*)data);
      //msgCounter++;
      //Serial.print("Msg count: ");
      //Serial.println(msgCounter);
      //Mirf.send(data);
    }
    else
    {
      // M_M
      if (CUR_DEBUG_LEVEL >= DEBUG_LEVEL_VERBOSE)
        Serial.println("Comm parse defered: Mirf Sending");
      // ***
    }
  }

}
void parseCommand(byte input[32])
{

  for ( int i = 0 ; i < 30; i++)
  {

    if ( input[i] == commandChar)
    {
      // M_M
      if (CUR_DEBUG_LEVEL >= DEBUG_LEVEL_VERBOSE)
        Serial.println("Comm char found:" + String(i));
      // ***

      //if ( ConfirmValues( input[i + 1] , input[i + 2]))
      WrtieToServo( input[i + 1] , input[i + 2]);
      i += 2;
    }
    else if (input[i] == 0)
    {
      // M_M
      if (CUR_DEBUG_LEVEL >= DEBUG_LEVEL_VERBOSE)
        Serial.println("End string found at:" + String(i));
      // ***
      break;
    }
  }

}

void WrtieToServo(byte channel , byte &value)
{

  // 0 through 9 is reserved
  int _value = value - 10;

  // 0 through 9 channels are reserved
  if ( _value < 180 && _value > 0) // filter out values that cant be mapped to servo
  {
    if (LERPENABLED)
    {
      switch ( channel)
      {
        case 10 :
          // Special lerp value for motor to fine tune starting torque based on design
          _value = int (lerp ( lasts[0] , _value , ESCLERPFACTOR));
          break;
        default:
          _value = int (lerp ( lasts[channel - 10] , _value , LERPFACTOR));
          break;
      }
    }


    switch ( channel)
    {
      case 10 :
        ESC.write (_value);
        break;
      case 11 :
        servo_1.write (_value);
        break;
      case 12 :
        servo_2.write (_value);
        break;
      case 13 :
        servo_3.write (_value);
        break;
      case 14 :
        servo_4.write (_value);
        break;
      case 15 :
        servo_5.write (_value);
        break;
      case 16 :
        //servo_4.write (_value);
        break;
      case 17 :
        //servo_4.write (_value);
        break;
      case 18 :
        //servo_4.write (_value);
        break;
      case 19 :
        //servo_4.write (_value);
        break;
      default:
        break;
    }
    // M_M
    //if (CUR_DEBUG_LEVEL >= DEBUG_LEVEL_VERBOSE)
    //  Serial.println("Servo" + String(channel - 10) + ": " + String(_value));
    // ***

    //if (LERPENABLED)
    {
      lasts[channel - 10] = _value; // save as last value
    }

  }
}

boolean ConfirmValues(byte channel , byte &value)
{
  byte _value = value - 10;
  channel -= 10;

  // detect noise
  if ( _value < lasts[channel] - NOISE_THR || _value > lasts[channel] + NOISE_THR)
  {
    if ( counters [channel] < RES_FACTOR )
    {
      counters[channel] += 1 ;
      return false;  // Resist noise, dont let servos move
    }
    else
    {
      counters[channel] = 0;
      return true;   // Maybe Its not noisy
    }
  }
  else
  {
    //lasts[channel] = _value; // save as last value
    return true;  // confirm data
  }
}

float lerp(float a, float b, float x)
{
  return a + x * (b - a);
}
