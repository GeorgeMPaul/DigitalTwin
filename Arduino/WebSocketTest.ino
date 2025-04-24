#include <WiFiS3.h>
#include <WebSocketServer.h>

using namespace net;

WebSocketServer wss(8080); // Changed port to 8080 for better compatibility
WiFiServer server(80);

const char ssid[] = "GEO";        // Change to your WiFi SSID
const char pass[] = "fishfries";  // Change to your WiFi password

int status = WL_IDLE_STATUS;

void setup() {
    Serial.begin(9600);
    delay(1000);  // Give time for Serial Monitor to start

    // Check firmware version
    String fv = WiFi.firmwareVersion();
    if (fv < WIFI_FIRMWARE_LATEST_VERSION) {
        Serial.println("Please upgrade the firmware.");
    }

    // Attempt to connect to WiFi with timeout
    Serial.print("Connecting to WiFi: ");
    Serial.println(ssid);

    unsigned long startAttemptTime = millis();
    while (WiFi.status() != WL_CONNECTED && millis() - startAttemptTime < 20000) {  // 20 sec timeout
        Serial.print(".");
        WiFi.begin(ssid, pass);
        delay(4000);
    }

    // Check if WiFi is connected
    if (WiFi.status() != WL_CONNECTED) {
        Serial.println("\nFailed to connect to WiFi. Restarting...");
        delay(5000);
        NVIC_SystemReset(); // Restart the board
    }

    Serial.println("\nConnected to WiFi!");
    Serial.print("IP Address: ");
    Serial.println(WiFi.localIP());

    server.begin();

    // WebSocket server setup
    wss.onConnection([](WebSocket &ws) {
        Serial.print("New WebSocket Connection from: ");
        Serial.println(ws.getRemoteIP());

        ws.onMessage([](WebSocket &ws, WebSocket::DataType dataType, const char *message, uint16_t length) {
            if (dataType == WebSocket::DataType::TEXT) {
                Serial.print("Received: ");
                Serial.println(message);

                // Respond to client
                String reply = "Server received: " + String(message);
                ws.send(WebSocket::DataType::TEXT, reply.c_str(), reply.length());
            }
        });

        ws.onClose([](WebSocket &, WebSocket::CloseCode, const char *, uint16_t) {
            Serial.println("Client disconnected");
        });

        const char welcomeMessage[] = "Hello from Arduino WebSocket Server!";
        ws.send(WebSocket::DataType::TEXT, welcomeMessage, strlen(welcomeMessage));
    });

    wss.begin();
    Serial.println("WebSocket server started on port 8080");
}

void loop() {
    wss.listen();  // Handle WebSocket connections
}
