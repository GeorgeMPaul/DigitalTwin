using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class ArduinoDataSubscriber : MonoBehaviour
{
    // Reference to the CommandSetter component
    [SerializeField] private CommandSetter commandSetter;

    void Start()
    {
        // Subscribe to the arduino_data topic
        ROSConnection.GetOrCreateInstance().Subscribe<StringMsg>("arduino_data", OnArduinoDataReceived);

        // If commandSetter isn't assigned in the inspector, try to find it
        if (commandSetter == null)
        {
            commandSetter = FindObjectOfType<CommandSetter>();
        }
    }

    // Callback function that is called whenever new data is received
    void OnArduinoDataReceived(StringMsg message)
    {
        // Print the received message to the Unity console
        Debug.Log("Received Arduino Data: " + message.data);

        // Update the instruction in CommandSetter
        if (commandSetter != null)
        {
            commandSetter.instruction = message.data;
        }
        else
        {
            Debug.LogError("CommandSetter reference not found!");
        }
    }
}