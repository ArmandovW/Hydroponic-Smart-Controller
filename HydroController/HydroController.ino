#include <Wire.h>
//#include <LiquidCrystal_I2C.h>
#include <OneWire.h>
#include <DallasTemperature.h>
#include "DHT.h"
#include "DFRobot_PH.h"
#include <EEPROM.h>

#define ONE_WIRE_BUS 5
#define DHTPIN 2
#define DHTTYPE DHT11
#define PH_PIN A0
// Optional: Define names for the relay states
//  Key Studio relays are typically active LOW
#define RELAY_OFF LOW   
#define RELAY_ON HIGH 

// Parameters //
//LCD
//LiquidCrystal_I2C lcd(0x27, 20, 4);
//DS18B20 Water Probe
OneWire oneWire(ONE_WIRE_BUS);
DallasTemperature sensors(&oneWire);
float Celcius=0;
//DHT11 Humidity and Temp sensor
DHT dht(DHTPIN, DHTTYPE);
float roomHumidity;
float roomTemp;
//PH Sensor
float voltage,phValue,temperature = 25;
DFRobot_PH ph;
//Button
const int buttonPin = 4;
const int relayPin = 8;
int buttonState = LOW;
int Pump = 0;

void setup() { 
  // initialize
  Serial.begin(9600);
//  lcd.begin();
  sensors.begin();
  dht.begin();
  ph.begin();
  pinMode(buttonPin, INPUT);
  pinMode(relayPin, OUTPUT);
  digitalWrite(relayPin, RELAY_OFF);
}
 
void loop() {
  //Read Serial
  String receivedString = Serial.readStringUntil('\n'); // Read until newline

    // Remove any leading/trailing whitespace (carriage return, etc.)
    receivedString.trim();

  if (receivedString.length() > 0 && receivedString.startsWith("PUMPON")) { //Check for empty strings
      Serial.print("Start Pump: ");
      Serial.print(receivedString);
      digitalWrite(relayPin, RELAY_ON);
      Pump = 1;
  }

  if (receivedString.length() > 0 && receivedString.startsWith("PUMPOFF")) { //Check for empty strings
      Serial.print("Disable Pump: ");
      Serial.print(receivedString);
      digitalWrite(relayPin, RELAY_OFF);
      Pump = 0;
  }

  //DS1820B
  sensors.requestTemperatures(); 
  Celcius=sensors.getTempCByIndex(0);
  Serial.print(Celcius);
  Serial.print(" C ");

  //DHT11 Humidity
  roomHumidity=dht.readHumidity();
  roomTemp=dht.readTemperature();
  delay(100);

  if (isnan(roomHumidity) || isnan(roomTemp)) {
    Serial.println(F("Failed to read from DHT sensor!"));
  }
  Serial.print(F(" Humidity: "));
  Serial.print(roomHumidity);
  Serial.print(F("%  Temperature: "));
  Serial.print(roomTemp);

  //PH sensor
      static unsigned long timepoint = millis();
    if(millis()-timepoint>1000U){                  //time interval: 1s
        timepoint = millis();
        //temperature = readTemperature();         // read your temperature sensor to execute temperature compensation
        voltage = analogRead(PH_PIN)/1024.0*5000;  // read the voltage
        phValue = ph.readPH(voltage,Celcius); // convert voltage to pH with temperature compensation
        ph.calibration(voltage,Celcius);
        Serial.print("  PH: ");
        Serial.print(phValue,2);
    }
  //Button
  buttonState = digitalRead(buttonPin);
  if (buttonState == HIGH) {
  Pump = !Pump;
  }

  if (Pump == 0) {
    Serial.println("  Pump Turned OFF  ");
  } else {
    Serial.println("  Pump Turned ON  ");
  }

  //LCD Value display
  // lcd.setCursor (0,0); //
  // lcd.print("WELCOME Armando");
  // lcd.setCursor (0,1); //
  // lcd.print("Temp:");
  // lcd.setCursor (5,1); //
  // lcd.print(Celcius);
  // lcd.setCursor (9,1);
  // lcd.print((char)223); 
  // lcd.setCursor (10,1);
  // lcd.print("C ");
  // lcd.setCursor (12,1); //
  // lcd.print("PH:");
  // lcd.setCursor (15,1); //
  // lcd.print(phValue,2);
  // lcd.setCursor (0,2); //
  // lcd.print("Humidity");
  // lcd.setCursor (9,2); //
  // lcd.print(roomHumidity);
  // lcd.setCursor (14,2); //
  // lcd.print("%");
  // lcd.setCursor (0,3); //
  // lcd.print("Room Temp:");
  // lcd.setCursor (10,3);
  // lcd.print(roomTemp);
  // lcd.setCursor (15,3);
  // lcd.print((char)223);

  delay(1000);

}
