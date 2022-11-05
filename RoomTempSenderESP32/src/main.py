# ampy --port COM3 put main.py
# ampy --port /dev/tty.usbserial-10 put main.py
from machine import Pin
from machine import deepsleep
from machine import ADC
from time import sleep
import urequests
import network
import json
import ubinascii

BATTERY_ADC_PIN= 35
MAX_TENTATIVE = 5
DEEP_SLEEP_TIME = 10000
SSID = 'SKYNETWIFI'
PWD = '  '
URL = 'http://192.168.2.40:5000/api/sensorData'

message = {
  "MAC": "",
  "Temp": 0.0,
  "Humidity": 0.0,
  "BatteryLevel": 0.0
}


def do_connect():   
    wlan = network.WLAN(network.STA_IF)
    wlan.active(True)
    if not wlan.isconnected():
        print('connecting to network...')
        try:
            wlan.connect(SSID, PWD)
            tentative = 0
            while not wlan.isconnected():
                tentative += 1
                if (tentative >= MAX_TENTATIVE):
                    print('Connection failed! DeepSleep')
                    deepsleep(DEEP_SLEEP_TIME)
                print('Connection failed!')
                sleep(1000)
        except:
            print("connect exception")
            deepsleep(DEEP_SLEEP_TIME)
                
    print('network config:', wlan.ifconfig())
    wlan_mac = wlan.config('mac')
    hex_mac = hexlify(wlan_mac, ':').decode().upper() 

def get_battery_level():
    battery_level = ADC(Pin(BATTERY_ADC_PIN))
    print(battery_level.read())
    # b=(adc.read_uv()/4096)*7.445
    # return(b)

 
# # led = Pin(5, Pin.OUT)
 
# # while True:
# #   led.value(not led.value())
# #   sleep(0.5)

# led = Pin (5, Pin.OUT)
# sleep(5)

# print('LED 1')
# led.value(1)
# sleep(5)
# print('LED 0')
# led.value(0)
# sleep(5)

# # wait 5 seconds so that you can catch the ESP awake to establish a serial communication later
# # you should remove this sleep line in your final script


# print('Im awake, but Im going to sleep')





# do_connect()
get_battery_level()


# # response = urequests.get("http://192.168.2.40:5000/api/Weather/80:7D:3A:57:F2:50")

# headers = {'Content-Type':'application/json'}
# data = (json.dumps(message)).encode()
# response = urequests.post(URL, data=data, headers=headers)
# # if response.status_code != 200:
           
# # print(response)
# # print(response.content)  
# # print(response.text)  
# # print(response.content)
# print(response.json()) 


# response.close()

# #sleep for 10 seconds (10000 milliseconds)
# # deepsleep(10000)



# from machine import Pin
# from machine import deepsleep
# from time import sleep

# import dht 

# sensor = dht.DHT22(Pin(21))

# while True:
#   try:
#     sleep(10)
#     sensor.measure()
#     temp = sensor.temperature()
#     hum = sensor.humidity()
#     print('Temperature: %3.1f C' %temp)
#     print('Humidity: %3.1f %%' %hum)
#     deepsleep(10000)
#   except OSError as e:
#     print('Failed to read sensor.')