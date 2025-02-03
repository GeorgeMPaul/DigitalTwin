using M2MqttUnity;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;

public class Motor_UnityMqtt : MqttUnityClient
{

    private string MQTTtopicName = "satorixr/digitaltwin/motor";

    protected override void DecodeMessage(string topic, byte[] message)
    {
        string angle = System.Text.Encoding.UTF8.GetString(message);
        Debug.Log("Motor rotated: " + angle);

        RotateMotor(angle);
    }

    private void RotateMotor(string angle)
    {
        if (float.TryParse(angle, out float degrees))
        {
            transform.Rotate(degrees, 0, 0);
        }
        else
        {
            Debug.LogWarning("Invalid input. Please enter a valid number.");
        }
    }

    protected override void SubscribeTopics()
    {
        client.Subscribe(new string[] { MQTTtopicName }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    }

    protected override void UnsubscribeTopics()
    {
        client.Unsubscribe(new string[] { MQTTtopicName });
    }

    protected override void Update()
    {
        base.Update(); // call ProcessMqttEvents()

    }
}
