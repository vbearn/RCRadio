/* YourDuinoStarter Example: nRF24L01 Radio remote control of servos by joystick
  - WHAT IT DOES
   Joystick on other Arduino communicates by nRF25L01 Radio to
   this Arduino with 2 pan-tilt servos
   SEE: The variable 'hasHardware'. You can test without servos and later set hasHardware = true;
        You NEED separate Servo power, not USB. YourDuino RoboRED has built in 2A power for servos
  - SEE the comments after "//" on each line below
  - CONNECTIONS:
   - nRF24L01 Radio Module: See http://arduino-info.wikispaces.com/Nrf24L01-2.4GHz-HowTo
   1 - GND
   2 - VCC 3.3V !!! NOT 5V
   3 - CE to Arduino pin 7
   4 - CSN to Arduino pin 8
   5 - SCK to Arduino pin 13
   6 - MOSI to Arduino pin 11
   7 - MISO to Arduino pin 12
   8 - UNUSED

  - V2.12 02/08/2016
   - Uses the RF24 Library by TMRH20 and Maniacbug: https://github.com/TMRh20/RF24 (Download ZIP)
   Questions: terry@yourduino.com */

/*-----( Import needed libraries )-----*/
#include <SPI.h>   // Comes with Arduino IDE
#include "RF24.h"  // Download and Install (See above)
#include "printf.h" // Needed for "printDetails" Takes up some memory
// NEED the SoftwareServo library installed
// http://playground.arduino.cc/uploads/ComponentLib/SoftwareServo.zip
//#include <SoftwareServo.h>  // Regular Servo library creates timer conflict!
/*-----( Declare Constants and Pin Numbers )-----*/
#define  CE_PIN  8   // The pins to be used for CE and SN
#define  CSN_PIN 7

#define ServoHorizontalPIN 4   //Pin Numbers for servos and laser/LED
#define ServoVerticalPIN   5
#define LaserPIN           6

#define ServoMIN_H  30  // Don't go to very end of servo travel
#define ServoMAX_H  150 // which may not be all the way from 0 to 180. 
#define ServoMIN_V  30  // Don't go to very end of servo travel
#define ServoMAX_V  150 // which may not be all the way from 0 to 180

/*-----( Declare objects )-----*/
/* Hardware configuration: Set up nRF24L01 radio on SPI bus plus (usually) pins 7 & 8 (Can be changed) */
RF24 radio(CE_PIN, CSN_PIN);

/*
SoftwareServo HorizontalServo;
SoftwareServo VerticalServo;  // create servo objects to control servos
*/
/*-----( Declare Variables )-----*/
byte addresses[][6] = {"1Node", "2Node"}; // These will be the names of the "Pipes"

// Allows testing of radios and code without servo hardware. Set 'true' when servos connected
boolean hasHardware = false;  // Allows testing of radios and code without Joystick hardware.
//boolean hasHardware = true;

int HorizontalJoystickReceived; // Variable to store received Joystick values
int HorizontalServoPosition;    // variable to store the servo position

int VerticalJoystickReceived;   // Variable to store received Joystick values
int VerticalServoPosition;      // variable to store the servo positio

/**
  Create a data structure for transmitting and receiving data
  This allows many variables to be easily sent and received in a single transmission
  See http://www.cplusplus.com/doc/tutorial/structures/
*/
struct dataStruct {
  unsigned long _micros;  // to save response times
  int Xposition;          // The Joystick position values
  int Yposition;
  bool switchOn;          // The Joystick push-down switch
} myData;                 // This can be accessed in the form:  myData.Xposition  etc.


unsigned long receivedPackCount = 0;

void setup()   /****** SETUP: RUNS ONCE ******/
{
  Serial.begin(115200);   // MUST reset the Serial Monitor to 115200 (lower right of window )
  // NOTE: The "F" in the print statements means "unchangable data; save in Flash Memory to conserve SRAM"
  Serial.println(F("YourDuino.com Example: Receive joystick data by nRF24L01 radio from another Arduino"));
  Serial.println(F("and control servos if attached (Check 'hasHardware' variable"));
  printf_begin(); // Needed for "printDetails" Takes up some memory
  /*-----( Set up servos )-----*/
  if (hasHardware)
  {
    /*HorizontalServo.attach(ServoHorizontalPIN);  // attaches the servo to the servo object
    VerticalServo.attach(ServoVerticalPIN);*/
  }

  radio.begin();          // Initialize the nRF24L01 Radio
  radio.setChannel(108);  // 2.508 Ghz - Above most Wifi Channels
  radio.setDataRate(RF24_250KBPS); // Fast enough.. Better range

  // Set the Power Amplifier Level low to prevent power supply related issues since this is a
  // getting_started sketch, and the likelihood of close proximity of the devices. RF24_PA_MAX is default.
  // PALevelcan be one of four levels: RF24_PA_MIN, RF24_PA_LOW, RF24_PA_HIGH and RF24_PA_MAX
  radio.setPALevel(RF24_PA_LOW);
  //   radio.setPALevel(RF24_PA_MAX);

  // Open a writing and reading pipe on each radio, with opposite addresses
  radio.openWritingPipe(addresses[1]);
  radio.openReadingPipe(1, addresses[0]);

  // Start the radio listening for data
  radio.startListening();
  radio.printDetails(); //Uncomment to show LOTS of debugging information
}//--(end setup )---


void loop()   /****** LOOP: RUNS CONSTANTLY ******/
{

  if ( radio.available())
  {

    while (radio.available())   // While there is data ready to be retrieved from the receive pipe
    {
      radio.read( &myData, sizeof(myData) );             // Get the data
    }

   // radio.stopListening();                             // First, stop listening so we can transmit
    //radio.write( &myData, sizeof(myData) );              
    /*if (!radio.write( &myData, sizeof(myData) )) {         // Send the received data back.
      Serial.println(F("Transmit failed "));
    }*/

   // radio.startListening();                              // Now, resume listening so we catch the next packets.
    receivedPackCount++;
    
    Serial.print(F("Packet Received - Sent response "));  // Print the received packet data
    Serial.print(receivedPackCount);
  
    Serial.print(" ");
    //Serial.print(myData._micros);
    //Serial.print(F("uS X= "));
    Serial.print(myData.Xposition);
    Serial.print(F(" Y= "));
    Serial.print(myData.Yposition);
    if ( myData.switchOn == 1)
    {
      Serial.println(F(" Switch ON"));
    }
    else
    {
      Serial.println(F(" Switch OFF"));
    }

  } // END radio available

  if (hasHardware)
  {
    /*-----( Calculate servo position values, send to the servos )-----*/
    /*
     SoftwareServo::refresh();//refreshes servo to keep them updating
     HorizontalJoystickReceived  = myData.Xposition;  // Get the values received
     VerticalJoystickReceived    = myData.Yposition;

     // scale it to use it with the servo (value between MIN and MAX)
     HorizontalServoPosition  = map(HorizontalJoystickReceived, 0, 1023, ServoMIN_H , ServoMAX_H);
     VerticalServoPosition    = map(VerticalJoystickReceived,   0, 1023, ServoMIN_V , ServoMAX_V);

     // tell servos to go to position
     HorizontalServo.write(HorizontalServoPosition);
     VerticalServo.write(VerticalServoPosition);*/
  } // END hasHardware
}//--(end main loop )---

/*-----( Declare User-written Functions )-----*/

// NONE YET
//*********( THE END )***********


