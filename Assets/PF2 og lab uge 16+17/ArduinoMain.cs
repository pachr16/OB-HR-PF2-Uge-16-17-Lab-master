﻿using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ArduinoMain : MonoBehaviour {
    //On included/premade Arduino functions:
    //delay(timeInMilliseconds) : use "yield return delay(timeInMilliseconds)", to get similar functionality as delay() in arduino would give you.

    //map() : works exactly as on Arduino, maps a long from one range to another. 
    //If you want to get an int or a float from the map()-function, you can cast the output like this: (int)map(s, a1, a2, b1, b2) or (float)map(s, a1, a2, b1, b2) 

    //millis() : returns the time as a ulong since the start of the scene (and therefore also the time since setup() was run) in milliseconds.

    //If you want to do something similar to serial.println(), use Debug.Log(). 

    //analogWrite() and analogRead() works as they do in arduino - remember to give them correct input-values.
    //digitalRead() and digitalWrite() writes and returns bools. (High = true). 
    //The console will display a "NotImplementedException" if you attempt to write to sensors or read from motors. 

    //Distance sensor:
    //The Distance (ultrasonic) sensor is added, if you use "pulseIn()" on the pin it is assigned to, 
    //it will return the time it took sound to travel double the distance to the point of impact in microseconds (type: ulong).
    //This mimics roughly how the HC-SR04 sensor works. 
    //There is no max-range of the distance-sensor. If it doesn't hit anything, it returns a 0.

    //Servo:
    //if you add the servo-prefab to the scene, ArduinoMain will automatically find the servo object, essentially handling "servo.attach()" automatically. 
    //There can be only one servo controlled by this script.
    //servo.write() and servo.read() implemented, they function similar to a servomotor. 
    //The angles that servo.write() can handle are [0:179]. All inputs outside of this range, are clamped within the range.
    //servo.read() will return the last angle written to the servo-arm. 
    //In order to attach something to the servo, so that it rotates with the servo-arm, simply make the object you wish to rotate, a child of either: Servo-rotationCenter or Servo-arm. 
    //Make sure to take into account the position of the object relative to Servo-rotationCenter. The rotated object will rotate positively around the Y-axis (up) of the Servo-rotationCenter gameobject.

    public Breadboard breadboard;
    public Servo servo;

    private int ldrLeft = 0, ldrRight = 1;
    private int forwardRight = 2, forwardLeft = 3;
    private int backRight = 4, backLeft = 5;
    private int distanceSensor = 6;
    private string state = "LineFollower";

    IEnumerator setup () {
        servo.write (90);

        //Example of delay:
        Debug.Log ("pre-delay log");
        yield return delay (2000); //2 second delay
        //Your code ends here -----

        //following region ensures delay-functionality for setup() & loop(). Do not delete, must always be last thing in setup.
        #region PremadeSetup
        yield return StartCoroutine (loop ());;
        #endregion PremadeSetup
    }

    IEnumerator loop () {

        if (state == "LineFollower") {
            if (pulseIn (distanceSensor) < 800 && pulseIn (distanceSensor) != 0) {
                stopAll ();
                Debug.Log ("Vi måler en væg! Så langt væk: " + pulseIn (distanceSensor));

                turn90Left ();
                yield return delay (1200); // enough delay to turn ~89-91 degrees, with some uncertainty due to Unity
                stopAll ();

                servo.write (88);
                yield return delay (2000);
                if (pulseIn (distanceSensor) < 1500) {
                    Debug.Log ("Changing state to WallAvoider!");
                    state = "WallAvoider";
                    yield return delay (1000);
                }

                if (state == "LineFollower") {
                    servo.write (180);
                    yield return delay (2000);
                    while (pulseIn (distanceSensor) == 0) {
                        analogWrite (forwardRight, 50);
                        analogWrite (forwardLeft, 50);
                        yield return delay (100);
                    }
                    stopAll ();

                    while (pulseIn (distanceSensor) < 1000) {
                        analogWrite (forwardLeft, 50);
                        analogWrite (forwardRight, 20);
                        yield return delay (10);

                        if (analogRead (ldrRight) < 100 || analogRead (ldrLeft) < 100) {
                            break;
                        }
                    }
                    stopAll ();
                }

            } else {
                if (analogRead (ldrLeft) > 900 && analogRead (ldrRight) > 900) {
                    driveForwards ();
                } else if (analogRead (ldrLeft) < 900 && analogRead (ldrRight) > 900) {
                    turnLeft ();
                } else if (analogRead (ldrLeft) > 900 && analogRead (ldrRight) < 900) {
                    turnRight ();
                }
                yield return delay (10);
            }

        } else if (state == "TESTING") {

        } else { // this is when we go to WallAvoider state
            Debug.Log ("We are now in WallAvoider state!");

            if (servo.read () != 179) {
                servo.write(90);
                yield return delay(250);
                while (pulseIn(distanceSensor) > 700) {
                    driveForwards();
                    yield return delay(10);
                }
                turnLeft();
                yield return delay(500);
                stopAll();
                servo.write (180);
                yield return delay (2000);
            }

            Debug.Log("We are " + pulseIn (distanceSensor) + " away from the right wall!");
            if (pulseIn (distanceSensor) > 600 || pulseIn (distanceSensor) == 0) {
                turnRight ();
            } else if (pulseIn (distanceSensor) < 500) {
                turnLeft ();
            } else {
                driveForwards ();
            }
            yield return delay (10);
        }

        //Following region is implemented as to allow "yield return delay()" to function the same way as one would expect it to on Arduino.
        //It should always be at the end of the loop()-function, and shouldn't be edited.
        #region DoNotDelete
        //Wait for one frame
        yield return new WaitForSeconds (0);
        //New loop():
        yield return loop ();
        #endregion DoNotDelete 
    }

    void turn90Left () {
        analogWrite (backLeft, 50);
        analogWrite (forwardRight, 50);
        //yield return delay(1200);       // enough delay to turn ~89-91 degrees, with some uncertainty due to Unity
        //stopAll();
    }

    void turn90Right () {
        analogWrite (backRight, 50);
        analogWrite (forwardLeft, 50);
        //yield return delay(1200);       // enough delay to turn ~89-91 degrees, with some uncertainty due to Unity
        //stopAll();
    }

    void turnLeft () {
        if (state == "LineFollower") {
            servo.write (45);
        }
        analogWrite (forwardLeft, 0); // denne er ikke nødvendig hvis vi i stedet kalder stopAll før vi begynder at dreje
        analogWrite (forwardRight, 50);
    }

    void turnRight () {
        if (state == "LineFollower") {
            servo.write (135);
        }
        analogWrite (forwardRight, 0); // denne er ikke nødvendig hvis vi i stedet kalder stopAll før vi begynder at dreje
        analogWrite (forwardLeft, 50);
    }

    void driveForwards () {
        if (state == "LineFollower") {
            servo.write (90);
        }
        analogWrite (forwardRight, 50);
        analogWrite (forwardLeft, 50);
    }

    void stopAll () {
        analogWrite (forwardRight, 0);
        analogWrite (forwardLeft, 0);
        analogWrite (backRight, 0);
        analogWrite (backLeft, 0);
    }

    #region PremadeDefinitions
    void Start () {
        servo = FindObjectOfType<Servo> ();
        if (servo == null) {
            Debug.Log ("No servo found in the scene. Consider assigning it to 'ArduinoMain.cs' manually.");
        }
        Time.fixedDeltaTime = 0.005f; //4x physics steps of what unity normally does - to improve sensor-performance.
        StartCoroutine (setup ());

    }

    IEnumerator delay (int _durationInMilliseconds) {
        float durationInSeconds = ((float) _durationInMilliseconds * 0.001f);
        yield return new WaitForSeconds (durationInSeconds);
    }

    public long map (long s, long a1, long a2, long b1, long b2) {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    public ulong millis () {
        return (ulong) (Time.timeSinceLevelLoad * 1000f);
    }

    public ulong abs (long x) {
        return (ulong) Mathf.Abs (x);
    }

    public long constrain (long x, long a, long b) {
        return (x < a ? a : (x > b ? b : x));
    }

    #endregion PremadeDefinitions

    #region InterfacingWithBreadboard
    public int analogRead (int pin) {
        return breadboard.analogRead (pin);
    }
    public void analogWrite (int pin, int value) {
        breadboard.analogWrite (pin, value);
    }
    public bool digitalRead (int pin) {
        return breadboard.digitalRead (pin);
    }
    public void digitalWrite (int pin, bool isHigh) {
        breadboard.digitalWrite (pin, isHigh);
    }

    public ulong pulseIn (int pin) {
        return breadboard.pulseIn (pin);
    }
    #endregion InterfacingWithBreadboard
}