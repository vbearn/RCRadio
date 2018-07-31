

#include <SPI.h>
#include <Mirf.h>
#include <nRF24L01.h>
#include <MirfHardwareSpiDriver.h>


// M_M
const byte DEBUG_LEVEL_NONE = 0;
const byte DEBUG_LEVEL_INFO = 1;
const byte DEBUG_LEVEL_VERBOSE = 2;

const byte CUR_DEBUG_LEVEL = 2;
// ***

void setup() {
  Serial.begin(115200);

  Serial.println("Configuring ... ");

  Mirf.spi = &MirfHardwareSpi;
  Mirf.init();
  Mirf.setTADDR((byte *)"pilot");
  Mirf.setRADDR((byte *)"remot");

  Mirf.payload = 32;
  Mirf.config();
  Mirf.send((byte*)"Remote Ready");
  Serial.println("Beginning ... ");
}

byte data[30];

void loop()
{
  int bufferLength = 0;

  while ( Serial.available() > 0)
  {

    bufferLength += Serial.readBytes(&data[bufferLength] , Serial.available());
    //Serial.readBytes( &data[i] , Serial.available());


    Serial.println("bff " + String(bufferLength));

    // M_M
    if (bufferLength >= 64)
    {
      if (CUR_DEBUG_LEVEL >= DEBUG_LEVEL_VERBOSE)
        Serial.println("ERR Serial buffer overflow");

      break;
    }
    // ***

  }

  if ( bufferLength >= 3 )
  {
    //byte msg[bufferLength];
    //memcpy(& msg, data, bufferLength);
    //Serial.print("Received : ");
    //Serial.println( (char*)data);
    Mirf.send(data);

    if (CUR_DEBUG_LEVEL >= DEBUG_LEVEL_VERBOSE)
      Serial.println("Sent buffer size: " + String(bufferLength));

    while (Mirf.isSending()) {
    }
    //Serial.println("Finished sending");
    delay(1);
  }

  bufferLength = 0;

  if ( Mirf.dataReady())
  {
    byte data[32];
    Mirf.getData(data );
    Serial.println( (char*)data);
  }


}



