#include <WiFiS3.h>
#include <PubSubClient.h>

// WiFi credentials
const char* ssid = "GEO";
const char* password = "fishfries";

// HiveMQ MQTT broker
const char* mqttServer = "broker.hivemq.com"; //"broker.hivemq.com" "broker.emqx.io"
const int mqttPort = 1883; //1883
const char* mqttUser = "";  // Optional, if using a secured broker
const char* mqttPassword = "";  // Optional, if using a secured broker

WiFiClient wifiClient;
PubSubClient client(wifiClient);

// Pin for the built-in LED
const int ledPin = LED_BUILTIN;
bool ledState = false;

// Function prototypes
void connectToWiFi();
void reconnect();
void sendLedState();

void setup() {
  Serial.begin(9600);
  
  // Set the LED pin as output
  pinMode(ledPin, OUTPUT);

  // Connect to WiFi
  connectToWiFi();

  // Set the MQTT server and callback
  client.setServer(mqttServer, mqttPort);

  // Ensure MQTT connection
  reconnect();
}

void loop() {
  if (!client.connected()) {
    reconnect();
  }
  client.loop();

  // Toggle the LED state
  ledState = !ledState;
  digitalWrite(ledPin, ledState ? HIGH : LOW);

  // Send LED state as sensor data
  sendLedState();

  delay(5000);  // Adjust delay as needed
}

void connectToWiFi() {
  Serial.print("Connecting to WiFi...");
  WiFi.begin(ssid, password);
  
  while (WiFi.status() != WL_CONNECTED) {
    delay(1000);
    Serial.print(".");
  }
  
  Serial.println("Connected to WiFi");
}

void reconnect() {
  // Loop until we're reconnected
  while (!client.connected()) {
    Serial.print("Attempting MQTT connection...");
    
    // Attempt to connect
    if (client.connect("ArduinoClient", mqttUser, mqttPassword)) {
      Serial.println("Connected to MQTT broker");
    } else {
      Serial.print("Failed, rc=");
      Serial.print(client.state());
      Serial.println(" try again in 5 seconds");
      // Wait 5 seconds before retrying
      delay(5000);
    }
  }
}

void sendLedState() {
  // Create a payload with the LED state
  String payload = ledState ? "LED is ON" : "LED is OFF";
  
  // Publish the data to the MQTT topic
  client.publish("led/sensor", payload.c_str());
  Serial.println("Published: " + payload);
}
