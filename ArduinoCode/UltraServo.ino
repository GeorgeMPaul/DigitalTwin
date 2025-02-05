#include <Servo.h>
#include <WiFiS3.h>
#include <PubSubClient.h>


// WiFi credentials
const char* ssid = "IOT-PROJ";//"GEO";"Harshitha"
const char* password = "pnpUY3cH";//;"fishfries";"12345678"
// HiveMQ MQTT broker
const char* mqttServer = "broker.hivemq.com";
const int mqttPort = 1883;
const char* mqttUser = "";
const char* mqttPassword = "";


const int trigPin = 10;
const int echoPin = 11;
float duration, distance;
const int servoPin = 9;
Servo myServo;

WiFiClient wifiClient;
PubSubClient client(wifiClient);


// Function Prototypes
void connectToWiFi();
void reconnect();
void sendSensorData(float distance);
void getUltrasonicDistance();
void mqttCallback(char* topic, byte* payload, unsigned int length);


void setup() {
  Serial.begin(9600);
  pinMode(trigPin, OUTPUT);
  pinMode(echoPin, INPUT);
  myServo.detach();
 
  connectToWiFi();
  client.setServer(mqttServer, mqttPort);
  client.setCallback(mqttCallback);
  reconnect();
}


void loop() {
  if (!client.connected()) {
    reconnect();
  }
  client.loop();  // Keep MQTT active
  getUltrasonicDistance();  // Get and publish ultrasonic data
}


// Function to measure distance using Ultrasonic Sensor
void getUltrasonicDistance() {
  digitalWrite(trigPin, LOW);
  delayMicroseconds(2);
  digitalWrite(trigPin, HIGH);
  delayMicroseconds(10);
  digitalWrite(trigPin, LOW);
  duration = pulseIn(echoPin, HIGH);
  distance = (duration * 0.0343) / 2;  // Convert to cm
  sendSensorData(distance);  // Send to MQTT
}


// Function to send ultrasonic distance data via MQTT
void sendSensorData(float distance) {
  String payload = String(distance);
  client.publish("satorixr/digitaltwin/motor", payload.c_str());
  Serial.println("Published Distance: " + payload);
}


void mqttCallback(char* topic, byte* payload, unsigned int length) {
  String receivedTopic = String(topic);                // Convert topic to String
  if (receivedTopic == "satorixr/digitaltwin/test") {  // Check if the topic matches
    String message = String((char*)payload).substring(0, length);
    int servoAngle = message.toInt();
    myServo.attach(servoPin);
    myServo.write(servoAngle);
    delay(500);
    Serial.print("Servo moved to: ");
    Serial.println(servoAngle);
    myServo.detach();
  } else {
    Serial.print("Message received on different topic: ");
    Serial.println(receivedTopic);
  }
}


void connectToWiFi() {
  Serial.print("Connecting to WiFi...");
  WiFi.begin(ssid, password);


  while (WiFi.status() != WL_CONNECTED) {
    delay(1000);
    Serial.print(".");
  }


  Serial.println("\nConnected to WiFi");
}


void reconnect() {
  while (!client.connected()) {
    Serial.print("Attempting MQTT connection...");


    if (client.connect("ArduinoClient", mqttUser, mqttPassword)) {
      Serial.println("Connected to MQTT broker");

      // Subscribe only to the required topic
      client.subscribe("satorixr/digitaltwin/test");
    } else {
      Serial.print("Failed, rc=");
      Serial.print(client.state());
      Serial.println(" Retrying in 5 seconds...");
      delay(5000);
    }
  }
}
