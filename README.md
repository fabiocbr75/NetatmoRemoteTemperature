# NetatmoRemoteTemperature
This project would like to resolve the problem related to not accurate temperature retrieved by radiator's sensor installed on valves. The problem is not related to netatmo sensor quality but to the nearness of the sensor to the radiator.
To achieve this target the solution use a remote temperature sensor for each valve and one single hub that receive all sensor data, save history and call netatmo API.

## Network Layout
  ![Network Layout](/NetworkLayout.png)

## Technology used:
- Asp.Net Core 2.2 / Docker / Raspberry pi 3
- ESP8266 / Arduino C++
- Netatmo public API
- SPA / Angular7 / ngx-admin / node-nginx / Docker

# Remote Sensor (Project status 100%)
- Arduino compatible board (Esp8266) to retrieve temperature and humidity (DHT22) or only temperature (DS18B20).

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
![FinalVersion1](/RoomTempSender/Images/FinalVersion1.png)

### Compile & Install:
 - Install Arduino IDE 1.8.6
 - Open /RoomTempSender/RoomTempSender/RoomTempSender.ino 
 - Use "Manage Libraries" to add the required libraries
 - Change the source code with network SSID, Password and TemperatureHub endpoint
 - Compile and upload to the device

### Use:
 - Remember to close the jumper after downloading the firmware into the board to awake from the deep sleep and to read the battery level. If you close the jumper for deep sleep you can't download the firmware.

![LolinSchema](/RoomTempSender/Images/LolinSchema.png)
 
 - Schema to connect the DS18B20 with the ESP8266 (in this case a NODEMCU). The components are Dallas DS18B20 temperature sensor and 4.7K resistor.
 
 ![DS18B20Schema](/RoomTempSender/Images/ESP8266_ds18b20_arduino.jpg)
 
 - Every 10 minutes the system send to TemperatureHub all sensor data and goes into deepsleep. The remote sensor consumption during works is around of 78ma while during deep sleep the consume is around of 0,067ma.
 The target goal is to recharge the battery every 50/60 days



# Temperature Hub (Project status 80%)
- Asp.net core 2.2 webapi application to save on SQLite all temperature retrieved.
- Switch to manual and set new temperature target based on remote temperature maintaining the scheduled target temperature

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
- On PC
  - Install\Check Visual Studio 2017 
  - Install\Check .net core 2.2
  - Install\Check Docker for Windows
  - Clone Git Repo
  - Move on /TemperatureHub
  - Run `docker build -t sensordata .`
  - Save image as tar `docker save sensordata > sensordata.tar`
  - Move tar file on Raspberry (ex. WinSCP)

- On Raspberry
  - Install Rasbian - Configure Network
  - Install Docker on Raspberry
    ```
     curl -fsSL get.docker.com -o get-docker.sh && sh get-docker.sh
     sudo groupadd docker
     sudo gpasswd -a $USER docker
     restart
    ```
  - Load Docker image `docker load < sensordata.tar`
  - Run Docker container (fill empty field)
    ```
    docker run -d -v ~/SensorData:/app/AppData  -p 5000:5000 -e TZ=Europe/Rome -e AppSettings:clientId='' -e AppSettings:clientSecret='' -e AppSettings:username='' -e AppSettings:password='' -e AppSettings:homeId='' --restart=always sensordata
    ```

Cross platform Arm\x86
To be define

### Use:

To be define

# Temperature UI (Project status 60%)
### Main activities:
- Show current temperature status and graph history

### Technology used:
- SPA / Angular7 / ngx-admin / node-nginx / Docker

### Hardware
- Hosted on TemperatureHub (Raspberry) on Docker container (node-nginx)

### Compile & Install:
- On PC
  - Install\Check Visual Studio Code
  - Install\Check node (v10)
  - Install\Angular7 cli
  - Install\Check Docker for Windows
  - Clone Git Repo
  - Move on /Dashboard
  - Run `docker build -t dashboard .`
  - Save image as tar `docker save dashboard > dashboard.tar`
  - Move tar file on Raspberry (ex. WinSCP)

- On Raspberry
  - Install Rasbian - Configure Network
  - Install Docker on Raspberry
    ```
     curl -fsSL get.docker.com -o get-docker.sh && sh get-docker.sh
     sudo groupadd docker
     sudo gpasswd -a $USER docker
     restart
    ```
  - Load Docker image `docker load < dashboard.tar`
  - Run Docker container (fill empty field)
    ```
	docker run -d -p 8080:80 -e TZ=Europe/Rome --restart=always dashboard
    ```


### Use:

To be define