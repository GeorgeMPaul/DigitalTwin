#include <MPU9250_WE.h>
#include <Wire.h>

#define MPU9250_ADDR 0x68

MPU9250_WE myMPU9250 = MPU9250_WE(MPU9250_ADDR);

// Thresholds
const float ACC_THRESHOLD = 0.8;      // Up/Down sensitivity
const float GYR_THRESHOLD = 100.0;    // Left/Right sensitivity

String lastVertical = "D";   // Default initial position
String lastHorizontal = "";  // Assume no initial rotation
bool lightON = false;        // LED state tracker

void setup() {
  Serial.begin(115200);
  Wire.begin();
  
  pinMode(LED_BUILTIN, OUTPUT); // Set the built-in LED as an output
  
  // Initialize MPU9250
  if (!myMPU9250.init()) {
    Serial.println("MPU9250 does not respond");
  } else {
    Serial.println("MPU9250 is connected");
  }
  
  if (!myMPU9250.initMagnetometer()) {
    Serial.println("Magnetometer does not respond");
  } else {
    Serial.println("Magnetometer is connected");
  }
  
  Serial.println("Position your MPU9250 flat and don't move it - calibrating...");
  delay(1000);
  myMPU9250.autoOffsets();
  Serial.println("Done!");
  
  // Configure MPU9250 settings
  myMPU9250.enableGyrDLPF();
  myMPU9250.setGyrDLPF(MPU9250_DLPF_6);
  myMPU9250.setSampleRateDivider(5);
  myMPU9250.setGyrRange(MPU9250_GYRO_RANGE_250);
  myMPU9250.setAccRange(MPU9250_ACC_RANGE_2G);
  myMPU9250.enableAccDLPF(true);
  myMPU9250.setAccDLPF(MPU9250_DLPF_6);
  myMPU9250.setMagOpMode(AK8963_CONT_MODE_100HZ);
  
  delay(200);
}

void loop() {
  // Part 1: Handle MPU9250 sensor readings
  readAndProcessMPU();
  
  // Part 2: Handle LED control via serial
  handleLEDControl();
  
  delay(200);
}

void readAndProcessMPU() {
  // Read sensor data
  xyzFloat acc = myMPU9250.getGValues();
  xyzFloat gyr = myMPU9250.getGyrValues();
  
  float accZ = acc.z * 9.81;  // Up/Down
  float gyrZ = gyr.z;         // Left/Right
  
  // Detect Up/Down
  if (accZ > (9.81 + ACC_THRESHOLD)) {
    lastVertical = "U";
  } else if (accZ < (9.81 - ACC_THRESHOLD)) {
    lastVertical = "D";
  }
  
  // Detect Left/Right
  if (gyrZ > GYR_THRESHOLD) {
    lastHorizontal = "R";
  } else if (gyrZ < -GYR_THRESHOLD) {
    lastHorizontal = "L";
  }
  
  // Print MPU9250 data
  //Serial.print("Position: ");
  Serial.println(lastVertical);
  if (lastHorizontal != "") {
    Serial.println(lastHorizontal);
  }
}

void handleLEDControl() {
  // Check for serial commands for LED
  if (Serial.available() > 0) {
    char receivedChar = Serial.read();
    if (receivedChar == '1') {
      lightON = true;
    }
    else if (receivedChar == '2') {
      lightON = false;
    }
  }
  
  // Update LED state
  if (lightON) {
    digitalWrite(LED_BUILTIN, HIGH);  // Turn LED ON
    Serial.flush();
    //Serial.println("LED ON");
  } else {
    digitalWrite(LED_BUILTIN, LOW);   // Turn LED OFF
    Serial.flush();
    //Serial.println("LED OFF");
  }
}