# NetatmoRemoteTemperature

- Arduino base board (NodeMCU Esp8266) to retrieve temperature and humidity (DHT22) or only temperature (DS18B20).
- Asp.net core 2.2 webapi application to save on SQLite all temperature retrieved.
- Switch to manual and set new temperature target based on remote temperature maintaining the scheduled target temperature

## Network Layout
  ![Network Layout](/NetworkLayout.png)

## Technology used:
- Asp.Net Core 2.2 / Docker / Raspberry pi 3
- ESP8266 / Arduino C++
- Netatmo public API
- SPA / Angular7 / jQWidgets Components

# Remote Sensor (Project status 80%)
### Main activities:
- Read Temperature \ Humidity
- Read battery level
- Connect to WiFi
- Send data to Rest API
- DeepSleep

### Technology used:
 - ESP8266 / Arduino C++
 - DHT Library
 - OneWire \ Dallas Library
 - ADC to check battery level
 - DeepSleep

### Hardware
- LOLIN D1 MINI PRO v2 -  4,00 €
- DHT22 or DS18B20 - 1,80 € / 0,50 €
- Lithion Battery 18650 3000ma - 4,00 €
- Other parts (box, battery holder, etc..) 2,00 €

Total price of around 11,00 € for each room\sensor (AliExpress)

![LOLIN D1 MINI PRO v2](/RoomTempSender/Images/d1_mini_pro_v2.png)
![DHT22](/RoomTempSender/Images/DHT22.png)
![DS18B20](/RoomTempSender/Images/ds18b20.png)
![Lithion Battery](/RoomTempSender/Images/Battery.png)
![Battery holder](/RoomTempSender/Images/BatteryHolder.png)
![Box](/RoomTempSender/Images/Box.png)

### Compile & Install:
 - Install Arduino IDE 1.8.6
 - Open /RoomTempSender/RoomTempSender/RoomTempSender.ino 
 - Use "Manage Libraries" to add the required libraries
 - Change the source code with network SSID, Password and TemperatureHub endpoint
 - Compile and upload to the device

### Use:
 - Every 10 minutes the system send to TemperatureHub all sensor data and goes into deepsleep

# Temperature Hub (Project status 80%)
### Main activities:
- Receive Remote Sensor Data
- Read Netatmo Schedule (Cached)
- Read Netatmo Valve Sensor Data (Cached)
- If needed change Netatmo Target Temperature

### Technology used:
- Asp.Net Core 2.2 / Cache / Sqlite / Docker / Raspberry pi 3
- Netatmo public API

### Hardware
- Raspberry Pi 3
- PowerSupply
- Case
- MicroSD 16Gb

Total price of around 57,00 € (Amazon)

![RaspberryKit](/RoomTempSender/Images/RaspberryKit.png)

### Compile & Install:
Install Rasbian - Configure Network
Install Docker
Cross platform Arm\x86
To be define

### Use:

To be define

# Temperature UI (Project status 20%)
### Main activities:
- Show temperature graph history

### Technology used:
- SPA / Angular7 / jQWidgets Components

### Hardware
- Hosted on TemperatureHub

### Compile & Install:
To be define

### Use:

To be define