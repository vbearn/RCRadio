
#include <SPI.h>
#include "RF24.h"  // Download and Install (See above)
#include "printf.h" // Needed for "printDetails" Takes up some memory


#define  CE_PIN  8   // The pins to be used for CE and SN
#define  CSN_PIN 7

/* Hardware configuration: Set up nRF24L01 radio on SPI bus plus (usually) pins 7 & 8 (Can be changed) */
RF24 radio(CE_PIN, CSN_PIN);

byte addresses[][6] = {"remot", "pilot"}; // These will be the names of the "Pipes"

const byte DEBUG_LEVEL_NONE = 0;
const byte DEBUG_LEVEL_INFO = 1;
const byte DEBUG_LEVEL_VERBOSE = 2;

const byte CUR_DEBUG_LEVEL = 2;

const byte commandChar = 244;

byte escmin = 0 ;
byte escmax = 180;
byte max1 = 150;
byte max2 = 150;
byte max3 = 155;
byte max4 = 155;
byte min1 = 30;
byte min2 = 30;
byte min3 = 25;
byte min4 = 25;
int offset1 = -17;
int offset2 = 5;
int offset3 = 4;
int offset4 = -2;


struct dataStruct {

  byte esc;
  byte servo1;
  byte servo2;
  byte servo3;
  byte servo4;
  byte servo5;
  byte servo6;

} myData;

unsigned long sentPackCount = 0;

void setup() {
  Serial.begin(115200);  // MUST reset the Serial Monitor to 115200 (lower right of window )

  Serial.println(F("Configuring ... "));

  printf_begin(); // Needed for "printDetails" Takes up some memory

  radio.begin();          // Initialize the nRF24L01 Radio
  radio.setChannel(108);  // Above most WiFi frequencies
  radio.setDataRate(RF24_250KBPS); // Fast enough.. Better range
  
  //radio.setPALevel(RF24_PA_MIN);
  //radio.setPALevel(RF24_PA_LOW);
  //radio.setPALevel(RF24_PA_HIGH);
  radio.setPALevel(RF24_PA_MAX);

  radio.openWritingPipe(addresses[0]);
  radio.openReadingPipe(1, addresses[1]);
  /*
    max1 += offset1;
    max2 += offset2;
    max3 += offset3;
    max4 += offset4;
    min1 -= offset1;
    min2 -= offset2;
    min3 -= offset3;
    min4 -= offset4;
    */
  // Start the radio listening for data
  //radio.startListening();

  radio.printDetails(); //Uncomment to show LOTS of debugging information

  //Mirf.send((byte*)"Remote Ready");
  Serial.println(F("Beginning ... "));
}

byte data[30];

byte TrimInRange(float value, byte minRange, byte maxRange) {
  if (value > maxRange)
    return maxRange;
  else if (value < minRange)
    return minRange;
  else
    return (byte)value;
}

void loop()
{
  float escInput = analogRead(A0);
  float input2 = analogRead(A1);
  float input3 = analogRead(A2);
  float input1 = analogRead(A3);
  // Map to Min - Max
  input1 = map(input1, 0, 1023, min1, max1);
  input2 = map(input2, 0, 1023, min2, max2);
  input3 = map(input3, 0, 1023, min3, max3);
  escInput = map(escInput, 0, 1023, escmax, escmin);

  myData.servo1 = (byte)map(input1, min1, max1, max1, min1);
  myData.servo2 = myData.servo1;//(byte)map(input1, min1, max1, max2, min2);

  byte elev1 = (byte)input3;
  byte elev2 = (byte)map(input3, min3, max3, max4, min4);

  float rudderDeviation = (float)input2 - ((float)max2 + (float)min2) / 2;
  byte rudderVator1 = TrimInRange(elev1 - rudderDeviation, min3, max3);
  byte rudderVator2 = TrimInRange(elev2 - rudderDeviation, min3, max3);

  myData.servo3 = rudderVator1;
  myData.servo4 = rudderVator2;

  // manage offsets
  myData.servo1 += offset1;
  myData.servo2 += offset2;//(byte)map(input1, min1, max1, max2, min2);
  myData.servo3 += offset3;
  myData.servo4 += offset4;





  myData.esc = (byte)escInput;

  if (CUR_DEBUG_LEVEL >= DEBUG_LEVEL_VERBOSE)
  {

    Serial.print(F("Sent, esc: "));
    Serial.print(String((int) myData.esc ));

    Serial.print(F(" servo1: "));
    Serial.print(String((int) myData.servo1 ));

    Serial.print(F(" servo2: "));
    Serial.print(String((int) myData.servo2 ));

    Serial.print(F(" servo3: "));
    Serial.print(String((int) myData.servo3 ));

    Serial.print(F(" servo4: "));
    Serial.print(String((int) myData.servo4 ));

    Serial.print(F(" servo5: "));
    Serial.print(String((int) myData.servo5 ));

    Serial.println();

  }
  radio.write( &myData, sizeof(myData) );
  /*
  int bufferLength = 0;

  while ( Serial.available() > 0)
  {

    bufferLength += Serial.readBytes(&data[bufferLength] , Serial.available());
    //Serial.readBytes( &data[i] , Serial.available());

    delay(10);
   // Serial.println("bff " + String(bufferLength));

    // M_M
    if (bufferLength >= 30)
    {
      if (CUR_DEBUG_LEVEL >= DEBUG_LEVEL_INFO)
        Serial.println(F("ERR Serial buffer overflow"));

      break;
    }
    // ***

  }

  if ( bufferLength >= 3 )
  {

    parseCommand(data);

    if (radio.write( &myData, sizeof(myData) )) {            // Send data, checking for error ("!" means NOT)

      sentPackCount++;
      if (CUR_DEBUG_LEVEL >= DEBUG_LEVEL_VERBOSE)
      {
        Serial.println(F("Sent count: "));
        Serial.print(sentPackCount);
      }
    }
    else {
      if (CUR_DEBUG_LEVEL >= DEBUG_LEVEL_INFO)
        Serial.println(F("Transmit failed "));
    }


    if (CUR_DEBUG_LEVEL >= DEBUG_LEVEL_VERBOSE)
    {

      Serial.print(F("Sent, esc: "));
      Serial.print(String((int) myData.esc ));

      Serial.print(F(" servo1: "));
      Serial.print(String((int) myData.servo1 ));

      Serial.print(F(" servo2: "));
      Serial.print(String((int) myData.servo2 ));

      Serial.print(F(" servo3: "));
      Serial.print(String((int) myData.servo3 ));

      Serial.print(F(" servo4: "));
      Serial.print(String((int) myData.servo4 ));

      Serial.print(F(" servo5: "));
      Serial.print(String((int) myData.servo5 ));

      Serial.println();

    }

    delay(20);
  }
  else {
    if ( bufferLength > 0)
      if (CUR_DEBUG_LEVEL >= DEBUG_LEVEL_INFO)
        Serial.println(F("buffer discarded"));
  }

  bufferLength = 0;
  */
}



void parseCommand(byte input[32])
{

  for ( int i = 0 ; i < 30; i++)
  {

    if ( input[i] == commandChar)
    {
      /*
      if (CUR_DEBUG_LEVEL >= DEBUG_LEVEL_VERBOSE) {
        Serial.print(F("Comm char found:"));
        Serial.println(String(i));
      }*/

      byte value = input[i + 2] - 10;

      switch ( (int)(input[i + 1]) )
      {
        case 10 :
          myData.esc =  value;
          break;
        case 11 :
          myData.servo1 =  value;
          break;
        case 12 :
          myData.servo2 =  value;
          break;
        case 13 :
          myData.servo3 =  value;
          break;
        case 14 :
          myData.servo4 =  value;
          break;
        case 15 :
          myData.servo5 =  value;
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
          if (CUR_DEBUG_LEVEL >= DEBUG_LEVEL_INFO)
            Serial.print(F("Invalid data in parse"));
          break;
      }

      i += 2;
    }
    else if (input[i] == 0)
    {
      // M_M
      if (CUR_DEBUG_LEVEL >= DEBUG_LEVEL_VERBOSE) {
        Serial.print(F("End string found at:"));
        Serial.println(String(i));
      }
      // ***
      break;
    }
  }

}

