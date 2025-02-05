using M2MqttUnity;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;

public class Kuka_UnityMqtt : MqttUnityClient
{
    private string MQTTtopicName = "satorixr/digitaltwin/kuka";
    private float rotationDuration = 2f; // Adjust this based on desired smoothness

    [System.Serializable]
    public class RobotAction
    {
        public string component;  
        public float degrees;     
    }

    protected override void DecodeMessage(string topic, byte[] message)
    {
        string msg = System.Text.Encoding.UTF8.GetString(message);

        RobotAction action = JsonUtility.FromJson<RobotAction>(msg);

        if (action != null)
        {
            GameObject componentObject = GameObject.Find(action.component);
            if (componentObject != null)
            {
                StartCoroutine(RotateComponentSmoothly(componentObject, action.degrees, rotationDuration));
            }
            else
            {
                Debug.LogWarning("Component not found: " + action.component);
            }
        }
        else
        {
            Debug.LogWarning("Failed to deserialize message.");
        }
    }

    private System.Collections.IEnumerator RotateComponentSmoothly(GameObject componentObject, float degrees, float duration)
    {
        // Get the current euler angle
        float currentAngle = componentObject.transform.eulerAngles.z;
        float currentAnglex = componentObject.transform.eulerAngles.x;
        float currentAngley = componentObject.transform.eulerAngles.y;
        float targetAngle = currentAngle + degrees;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float angle = Mathf.LerpAngle(currentAngle, targetAngle, elapsedTime / duration);
            componentObject.transform.rotation = Quaternion.Euler(currentAnglex, currentAngley, angle);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        componentObject.transform.rotation = Quaternion.Euler(currentAnglex, currentAngley, targetAngle);
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
