using System;
using System.IO.Ports;
using UnityEngine;

public class SerialReader : MonoBehaviour
{
    // SerialPort object to handle the communication
    private SerialPort serialPort;
    public string portName = "COM3";  // Change if necessary
    public int baudRate = 9600;       // Baud rate (9600 in this case)
    public int readTimeout = 500;     // Timeout in milliseconds

    // Data buffer for incoming data
    private string receivedData = "";

    void Start()
    {
        // Initialize and open the serial port
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = readTimeout;
            serialPort.Open();
            Debug.Log("Listening on " + portName + " at " + baudRate + " baud...");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error opening the port: " + ex.Message);
        }
    }

    void Update()
    {
        // Continuously check if there is any data available to read
        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                // Read available data from the serial port
                if (serialPort.BytesToRead > 0)
                {
                    receivedData = serialPort.ReadExisting(); // Read all available data
                    Debug.Log("Data received: " + receivedData);
                }
            }
            catch (TimeoutException)
            {
                // Timeout is expected if no data is available within the timeout period
            }
        }
    }

    void OnApplicationQuit()
    {
        // Close the serial port when the application quits
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("Serial port closed.");
        }
    }
}
