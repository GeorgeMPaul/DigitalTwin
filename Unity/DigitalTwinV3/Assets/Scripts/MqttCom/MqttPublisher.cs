using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace M2MqttUnity.Examples
{
    public class MqttPublisher : MonoBehaviour
    {
        [Header("MQTT Settings")]
        public string brokerAddress = "broker.hivemq.com";
        public int brokerPort = 1883;
        public string topicName = "satorixr/digitaltwin/test";
        public bool autoConnect = true;

        [Header("UI Elements")]
        public InputField messageInput;
        public Button publishButton;
        public TMP_Text statusText;

        private MqttClient client;
        private bool isConnected = false;

        private void Start()
        {
            if (publishButton != null)
                publishButton.onClick.AddListener(PublishMessage);

            if (autoConnect)
                Connect();
        }

        public void Connect()
        {
            StartCoroutine(DoConnect());
        }

        private IEnumerator DoConnect()
        {
            yield return new WaitForEndOfFrame();

            try
            {
                client = new MqttClient(brokerAddress, brokerPort, false, null, null, MqttSslProtocols.None);
                string clientId = Guid.NewGuid().ToString();
                client.Connect(clientId);

                if (client.IsConnected)
                {
                    Debug.Log($"Connected to MQTT broker at {brokerAddress}:{brokerPort}");
                    isConnected = true;
                    UpdateStatus("Connected to broker");
                }
                else
                {
                    Debug.LogError("Failed to connect to MQTT broker");
                    UpdateStatus("Connection Failed");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error connecting to MQTT broker: {e.Message}");
                UpdateStatus($"Error: {e.Message}");
            }
        }

        public void Disconnect()
        {
            if (client != null && client.IsConnected)
            {
                client.Disconnect();
                isConnected = false;
                Debug.Log("Disconnected from MQTT broker");
                UpdateStatus("Disconnected");
            }
        }

        public void PublishMessage()
        {
            if (!isConnected)
            {
                Debug.LogWarning("Not connected to MQTT broker");
                UpdateStatus("Not connected to broker");
                return;
            }

            if (messageInput == null || string.IsNullOrEmpty(messageInput.text))
            {
                Debug.LogWarning("No message to publish");
                UpdateStatus("No message to publish");
                return;
            }

            try
            {
                byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(messageInput.text);
                client.Publish(topicName, messageBytes, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);

                Debug.Log($"Published message: {messageInput.text} to topic: {topicName}");
                UpdateStatus($"Published: {messageInput.text}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error publishing message: {e.Message}");
                UpdateStatus($"Publish Error: {e.Message}");
            }
        }

        private void UpdateStatus(string status)
        {
            if (statusText != null)
                statusText.text = status;
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }

        private void OnDestroy()
        {
            Disconnect();
        }
    }
}
