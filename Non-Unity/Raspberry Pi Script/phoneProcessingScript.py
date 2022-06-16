#Script meant to process GPIO input from Rotary phone, convert it to a digit and then send that digit over a USB Keystroke

import RPi.GPIO as GPIO #Used to process GPIO inputs and enable power out
from time import sleep #Used to time the pulse readings
pulse = 32  #GPIO 12, Corresponds to pin for pulse created by dial spin
gate = 36 #GPIO 16, Corresponds to pin that opens switch when dial is being retracted
hangup = 40 #GPIO 21, Corresponds to the pin that closes circut when phone is on stand
gateOpen = False #Default state of gate is false
phoneLifted = False #Is phone lifted

GPIO.setwarnings(False) # Ignore warning in final version
GPIO.setmode(GPIO.BOARD) # Use physical pin numbering

#Set both recieving pins to PUD_DOWN state
GPIO.setup(pulse, GPIO.IN, pull_up_down=GPIO.PUD_DOWN)
GPIO.setup(gate, GPIO.IN, pull_up_down=GPIO.PUD_DOWN)
GPIO.setup(hangup, GPIO.IN, pull_up_down=GPIO.PUD_DOWN)

NULL_CHAR = chr(0) #Placeholder for empty character
count = 0 #Staring value for count

def writeToUSB(bCode): #Used to send the USB Keystroke
    try:
        with open('/dev/hidg0', 'rb+') as fd:
            fd.write(bCode.encode())
    except (RuntimeError, TypeError, NameError, BlockingIOError, BrokenPipeError):
        print("That didnt work lol")
        pass

def sendTheMessage(numberPass):
    keyCode = chr(0) #Default keycode if phone is interfered with in pulse stage
    #Following is just an if loop for selecting the correct KeyCode, corresponding to different USB Keyboard Signals
    if(numberPass == 1):
        keyCode = 30
    elif(numberPass == 2):
        keyCode = 31
    elif(numberPass == 3):
        keyCode = 32
    elif(numberPass == 4):
        keyCode = 33
    elif(numberPass == 5):
        keyCode = 34
    elif(numberPass == 6):
        keyCode = 35
    elif(numberPass == 7):
        keyCode = 36
    elif(numberPass == 8):
        keyCode = 37
    elif(numberPass == 9):
        keyCode = 38
    elif(numberPass == 10):
        keyCode = 39
    elif(numberPass == 11):
        keyCode = 11
    elif(numberPass == 12):
        keyCode = 15
    else:
        keyCode = 0
    print(numberPass) #Just used for feedback when running while connected to a machine


    writeToUSB(NULL_CHAR*2+chr(keyCode)+NULL_CHAR*5) #Sends the keycode determined by numberPass, the number of pulses
    writeToUSB(NULL_CHAR*8) #Sends release key signal to prevent duplicate keypresses

#Main loop of code
while True: # Run indefinitely
    
    if(not GPIO.input(hangup) and not phoneLifted):
        print("Phone lifted")
        phoneLifted = True
        sendTheMessage(12)
        sleep(0.07)
    if(GPIO.input(hangup) and phoneLifted):
        print("Phone has been put down")
        phoneLifted = False
        sendTheMessage(11)
        sleep(0.07)
    if(GPIO.input(pulse) == 0): #If pulse closes, this indicates a pulse, runs once for every pulse
        print("pulse")
        count += 1
        sleep(0.07) #Sleep matches pulse timings to acquire accurate counts
    if(not gateOpen and GPIO.input(gate) == 1): #Initial function to open the gate and begin the counting of pulses
        print("Gate is open")
        gateOpen = True
        count = 0 #Sets count to zero to begin count
        sleep(0.07)
    elif(gateOpen and GPIO.input(gate) == 0): #Triggers once dial spinback is complete, sending the final count of pulses onwards through the script
        print("Gate has been closed")
        gateOpen = False;
        sendTheMessage(count)
        sleep(0.07)
    
    
