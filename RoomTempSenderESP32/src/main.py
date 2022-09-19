# ampy --port COM3 put main.py

import urequests

# from machine import Pin
# from machine import deepsleep
# from time import sleep
 
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




response = urequests.get("http://192.168.2.40:5000/api/Weather/80:7D:3A:57:F2:50")


response.close()


#sleep for 10 seconds (10000 milliseconds)
# deepsleep(10000)