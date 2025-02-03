using M2MqttUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;

public class Led_UnityMqtt : MqttUnityClient
{
    public Material ledON;
    public Material ledOFF;
    public new Renderer renderer;

    private string MQTTtopicName = "satorixr/digitaltwin/led";

    protected override void DecodeMessage(string topic, byte[] message)
    {
        string ledstate = System.Text.Encoding.UTF8.GetString(message);
        SwitchLED(ledstate);
    }

    private void SwitchLED(string ledstate)
    {
        Debug.Log("LED state: " + ledstate);
        if (bool.TryParse(ledstate, out bool state))
        {
            if (state)
            {
                Debug.Log("Led "+state);
                SwitchMaterial(ledON);
            }
            else
            {
                Debug.Log("Led " + state);
                SwitchMaterial(ledOFF);
            }
        }
        else
        {
            Debug.LogWarning("Invalid input. Please enter a valid number.");
        }
    }

    public void SwitchMaterial(Material newMaterial)
    {
        if (renderer != null && newMaterial != null)
        {
            renderer.material = newMaterial;
        }
        else
        {
            Debug.LogWarning("Renderer or new material is missing.");
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
