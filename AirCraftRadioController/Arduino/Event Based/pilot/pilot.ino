
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
  if (!Mirf.isSending() && Mirf.dataReady())
  {
    byte data[32];
    //Serial.println("msg ready");
    Mirf.getData(data);

    parseCommand(data);

    //String msgBuffer = String ((char*)data);
    //msgCounter++;
    //Serial.print("Msg count: ");
    //Serial.println(msgCounter);
    //Mirf.send(data);

  }
}
void parseCommand( byte input[32])
{
  //int _length = sizeof(input);
  Serial.println((char*) input);
  for ( int i = 0 ; i < 32; i++)
  {
    //Serial.println();
    //Serial.print((char*)input[i]);
    //Serial.print(input[i], DEC);

    if ( input[i] == commandChar)
    {
      //Serial.println("com");
      if ( i < 30 )
      {
        //if ( ConfirmValues( input[i + 1] , input[i + 2]))
          WrtieToServo( input[i + 1] , input[i + 2]);
        i += 2;
      }
      else break;
    }
    //else if ( input[i] == 0)
    //  break;
  }
  //Serial.println();
}

void WrtieToServo(byte channel , byte &value)
{
  //Serial.println("writing");
  // 0 through 9 is reserved
  int _value = value - 10;
  //sendData(String(value));
  //sendData(String(_value));
  // 0 through 9 channels are reserved
  if ( _value < 180 && value > 0) // filter out values that cant be mapped to servo
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
     
    switch ( channel)
    {
      case 10 :
        ESC.write (_value);
        //Serial.println("wr1:" + String(_value));
        break;
      case 11 :
        servo_1.write (_value);
        //Serial.println("wr2:" + String(_value));
        break;
      case 12 :
        servo_2.write (_value);
        //Serial.println("wr3:" + String(_value));
        break;
      case 13 :
        servo_3.write (_value);
        //Serial.println("wr4:" + String(_value));
        break;
      case 14 :
        servo_4.write (_value);
        //Serial.println("wr5:" + String(_value));
        break;
      case 15 :
        servo_5.write (_value);
        //Serial.println("wr6:" + String(_value));
        break;
      case 16 :
        //servo_4.write (_value);
        //Serial.println("wr7:" + String(_value));
        break;
      case 17 :
        //servo_4.write (_value);
        //Serial.println("wr8:" + String(_value));
        break;
      case 18 :
        //servo_4.write (_value);
        //Serial.println("wr9:" + String(_value));
        break;
      case 19 :
        //servo_4.write (_value);
        //Serial.println("wr10:" + String(_value));
        break;
      default:
        break;
    }
    lasts[channel - 10] = _value; // save as last value
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
