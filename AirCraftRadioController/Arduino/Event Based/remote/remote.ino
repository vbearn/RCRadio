

#include <SPI.h>
#include <Mirf.h>
#include <nRF24L01.h>
#include <MirfHardwareSpiDriver.h>

void setup() {
  Serial.begin(115200);

  Mirf.spi = &MirfHardwareSpi;
  Mirf.init();
  Mirf.setTADDR((byte *)"pilot");
  Mirf.setRADDR((byte *)"remot");

  Mirf.payload = 32;
  Mirf.config();
  Mirf.send((byte*)"Remote Ready");
  Serial.println("Beginning ... ");
}
int i = 0;
byte data[30] ;
void loop() 
{
  while ( Serial.available() > 0 )
  {
    
    
    int bufferLength = Serial.readBytes( &data[i] , Serial.available());
    //Serial.readBytes( &data[i] , Serial.available());
    
    i+= bufferLength;

  }
  if ( i >= 3 )
  {
    byte msg[i];
    memcpy(& msg, data, i);
    //Serial.print("Received : ");
    //Serial.println( (char*)data);
    Mirf.send(msg);
    
    i = 0;
    while (Mirf.isSending()) {
    }
    //Serial.println("Finished sending");
    delay(1);
  }
  
  if ( Mirf.dataReady())
  {
    byte data[32];
    Mirf.getData(data );
    Serial.println( (char*)data);

    //delay(1000);
  }
  
  
}



