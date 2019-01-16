#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
#include <ArduinoJson.h>

//#define DEBUG
#define USE_DS18B20
#define CHECK_BATTERY

const char* ssid     = "SKYNETWIFI";
const char* password = "";

//const char* host = "http://192.168.2.63:5000/api/sensorData";
const char* host = "http://192.168.2.40:5000/api/sensorData";
String macAddress;


#ifdef DEBUG
  #define DEBUG_PRINT(x)    Serial.print (x)
  #define DEBUG_PRINTDEC(x) Serial.print (x, DEC)
  #define DEBUG_PRINTLN(x)  Serial.println (x)
#else
  #define DEBUG_PRINT(x)
  #define DEBUG_PRINTDEC(x)
  #define DEBUG_PRINTLN(x) 
#endif

#ifdef USE_DS18B20
  #include <OneWire.h>
  #include <DallasTemperature.h>
  #define ONE_WIRE_BUS 4 //Pin to which is attached a temperature sensor

  OneWire oneWire(ONE_WIRE_BUS);
  DallasTemperature DS18B20(&oneWire);  

  float GetTemperature()
  {
    float t = 0;
    int retry = 0;
    do 
    {
      if (retry > 10)
      {
        DEBUG_PRINT("NO TEMPERATURE DATA. Going into deep sleep for 600 seconds - 10 minutes");
        ESP.deepSleep(600e6);       
      }
      
      if (retry > 0)
      {
        delay(1000);  
        DEBUG_PRINTLN("Failed to read T from DS18B20 sensor!");
      }

      DS18B20.requestTemperatures(); 
      t = DS18B20.getTempCByIndex(0);

      retry++;
    } while (t == 85.0 || t == (-127.0));
    return t;
  }
#else
  #include "DHT.h"
  #define DHTPIN 4     // what digital pin the DHT22 is conected to
  #define DHTTYPE DHT22   // there are multiple kinds of DHT sensors
//#define DHTPIN 2     // what digital pin the DHT11 is conected to
//#define DHTTYPE DHT11   // there are multiple kinds of DHT sensors
  DHT dht(DHTPIN, DHTTYPE);
 
  float GetTemperature()
  {
    float h = 0;
    float t = 0;
    int retry = 0;
    
    do
    {
      
      if (retry > 10)
      {
        DEBUG_PRINT("NO TEMPERATURE DATA. Going into deep sleep for 600 seconds - 10 minutes");
        ESP.deepSleep(600e6);       
      }      
      
      if (retry > 0)
      {
        delay(1000);  
        DEBUG_PRINTLN("Failed to read T from DHT sensor!");
      }
  
      t = dht.readTemperature();
      retry++;
      
    } while (isnan(t));

  /*
    retry = 0;
    do
    {
      if (retry > 0)
      {
        delay(500);  
        DEBUG_PRINTLN("Failed to read H from DHT sensor!");
      }
  
      h = dht.readHumidity();
      
      retry++;
    } while (isnan(t));
  
    DEBUG_PRINT("Humidity: ");
    DEBUG_PRINT(h);
    DEBUG_PRINT(" %\t");
    */
    return t;
  }
  
#endif


 float GetBatteryLevel() 
 {
    unsigned int raw=0;
    float volt = 0.0;
    
    pinMode(A0, INPUT);
    raw = analogRead(A0);
    volt=raw/1023.0;
    volt=volt*4.5;
    
    return volt;
 }

void setup() {
  float h = 0.0;
  float t = 0.0;
  float v = 0.0;
  int retry = 0;
  
  Serial.begin(74880);
  delay(100);
 
  // We start by connecting to a WiFi network

  DEBUG_PRINTLN();
  DEBUG_PRINTLN();
  DEBUG_PRINT("Connecting to ");
  DEBUG_PRINTLN(ssid);
  
  WiFi.persistent(true);
  WiFi.mode(WIFI_STA);
  WiFi.begin(ssid, password);
  
  
  while (WiFi.status() != WL_CONNECTED) 
  {
    delay(1000);
    DEBUG_PRINT(".");
    retry++;
    if (retry > 10)
    {
      DEBUG_PRINT("NO WIFI Connection. Going into deep sleep for 600 seconds - 10 minutes");
      ESP.deepSleep(600e6);       
    }
  }

  DEBUG_PRINTLN("");
  DEBUG_PRINTLN("WiFi connected");
  DEBUG_PRINTLN("IP address: ");
  DEBUG_PRINTLN(WiFi.localIP());
  DEBUG_PRINT("MAC: ");
  macAddress = WiFi.macAddress();
  DEBUG_PRINTLN(macAddress);  
  DEBUG_PRINTLN("Start reading!");  

  t = GetTemperature();
    
  DEBUG_PRINT("Temperature: ");
  DEBUG_PRINT(t);
  DEBUG_PRINT(" *C ");

 #ifdef CHECK_BATTERY
    DEBUG_PRINT(" %\t");
    v = GetBatteryLevel();
 #endif
  
  DEBUG_PRINT("BatteryLevel: ");
  DEBUG_PRINT(v);
  DEBUG_PRINT(" V ");

  DEBUG_PRINT("connecting to ");
  DEBUG_PRINTLN(host);
  HTTPClient http;
  
  StaticJsonBuffer<200> jsonBuffer;

  JsonObject& root = jsonBuffer.parseObject("{\"MAC\" : \"\",\"Temp\": 0.0,\"Humidity\": 0.0,\"BatteryLevel\": 0.0}"); 

  root["MAC"] = macAddress;
  root["Temp"] = t;
  root["Humidity"] = h;
  root["BatteryLevel"] = v;
 
  http.begin(host);
  http.addHeader("Content-Type", "application/json"); //Specify content-type header

  char JSONmessageBuffer[300];
  root.prettyPrintTo(JSONmessageBuffer, sizeof(JSONmessageBuffer));
  
  int httpCode = http.POST(JSONmessageBuffer); 
  DEBUG_PRINTLN(httpCode);
  
  if (httpCode > 0) { //Check the returning code
    String payload = http.getString();   //Get the request response payload
    DEBUG_PRINTLN(payload);             //Print the response payload
  }
  
  DEBUG_PRINTLN(JSONmessageBuffer);

  http.end();   //Close connection
  
  DEBUG_PRINTLN("closing connection");
  DEBUG_PRINTLN("");
  Serial.flush();

  DEBUG_PRINT("Going into deep sleep for 600 seconds - 10 minutes");
  ESP.deepSleep(600e6); 
  //ESP.deepSleep(20e6); 
}

void loop() {

}
