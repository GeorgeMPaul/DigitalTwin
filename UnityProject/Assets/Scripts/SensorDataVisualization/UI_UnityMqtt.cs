using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using uPLibrary.Networking.M2Mqtt.Messages;


namespace M2MqttUnity.Examples
{
    public class UI_UnityMqtt : MqttUnityClient
    {
        private string MQTTtopicName = "satorixr/digitaltwin/#";

        public InputField consoleInputField;
        public InputField addressInputField;
        public InputField portInputField;
        public InputField sendDataInputField;
        public UnityEngine.UI.Button connectButton;
        public UnityEngine.UI.Button disconnectButton;
        public UnityEngine.UI.Button testPublishButton;
        public UnityEngine.UI.Button clearButton;

        private List<string> eventMessages = new List<string>();
        private bool updateUI = false;

        public void TestPublish()
        {
            var testdata = sendDataInputField.text;
            Debug.Log(testdata);
            client.Publish(MQTTtopicName.Replace("#", "test"), System.Text.Encoding.UTF8.GetBytes(testdata), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            AddUiMessage("Test message published.");
        }

        public void SetUiMessage(string msg)
        {
            if (consoleInputField != null)
            {
                consoleInputField.text = msg;
                updateUI = true;
            }
        }

        public void AddUiMessage(string msg)
        {
            if (consoleInputField != null)
            {
                consoleInputField.text += msg + "\n";
                updateUI = true;
            }
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            SetUiMessage("Connected to broker on " + brokerAddress + "\n");
        }


        protected override void OnConnectionFailed(string errorMessage)
        {
            AddUiMessage("CONNECTION FAILED! " + errorMessage);
        }

        protected override void OnDisconnected()
        {
            AddUiMessage("Disconnected.");
        }

        protected override void OnConnectionLost()
        {
            AddUiMessage("CONNECTION LOST!");
        }

        private void UpdateUI()
        {
            if (client == null)
            {
                addressInputField.text = brokerAddress;
                portInputField.text = brokerPort.ToString();
                if (connectButton != null)
                {
                    connectButton.interactable = true;
                    disconnectButton.interactable = false;
                    testPublishButton.interactable = false;
                }
            }
            else
            {
                if (testPublishButton != null)
                {
                    testPublishButton.interactable = client.IsConnected;
                }
                if (disconnectButton != null)
                {
                    disconnectButton.interactable = client.IsConnected;
                }
                if (connectButton != null)
                {
                    connectButton.interactable = !client.IsConnected;
                }
            }
            if (sendDataInputField != null && connectButton != null)
            {
                sendDataInputField.interactable = !connectButton.interactable;
            }
            if (clearButton != null && connectButton != null)
            {
                clearButton.interactable = connectButton.interactable;
            }
            updateUI = false;
        }

        protected override void Start()
        {
            SetUiMessage("Ready.");
            updateUI = true;
            base.Start();
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            string msg = System.Text.Encoding.UTF8.GetString(message);
            StoreMessage(msg);
        }

        private void StoreMessage(string eventMsg)
        {
            eventMessages.Add(eventMsg);
        }

        private void ProcessMessage(string msg)
        {
            AddUiMessage(msg);
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
            if (eventMessages.Count > 0)
            {
                foreach (string msg in eventMessages)
                {
                    ProcessMessage(msg);
                }
                eventMessages.Clear();
            }
            if (updateUI)
            {
                UpdateUI();
            }
        }

        private void OnDestroy()
        {
            Disconnect();
        }
    }
}
