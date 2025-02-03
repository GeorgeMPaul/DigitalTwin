using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace M2MqttUnity
{
    public class MqttUnityClient : MonoBehaviour
    {
        public string brokerAddress = "broker.hivemq.com";
        public int brokerPort = 1883;
        public bool autoConnect = true;

        private string mqttUserName = null;//"Unity";
        private string mqttPassword = null;//"Paul123!";

        private bool isEncrypted = false;
        private string certificateName;

        private int connectionDelay = 500; //Connection timeout in milliseconds
        private int timeoutOnConnection = MqttSettings.MQTT_CONNECT_TIMEOUT;

        protected MqttClient client;

        private List<MqttMsgPublishEventArgs> messageQueue1 = new List<MqttMsgPublishEventArgs>();
        private List<MqttMsgPublishEventArgs> messageQueue2 = new List<MqttMsgPublishEventArgs>();
        private List<MqttMsgPublishEventArgs> frontMessageQueue = null;
        private List<MqttMsgPublishEventArgs> backMessageQueue = null;
        private bool mqttClientConnectionClosed = false;
        private bool mqttClientConnected = false;


        public event Action ConnectionSucceeded;// Event fired when a connection is successfully established
        public event Action ConnectionFailed;// Event fired when failing to connect

        public virtual void Connect() // Connect to the broker using current settings.
        {
            if (client == null || !client.IsConnected)
            {
                StartCoroutine(DoConnect());
            }
        }

        public virtual void Disconnect() // Disconnect from the broker, if connected.
        {
            if (client != null)
            {
                StartCoroutine(DoDisconnect());
            }
        }

        protected virtual void OnConnecting() // Override this method to take some actions before connection (e.g. display a message)
        {
            //Debug.LogFormat("Connecting to broker on {0}:{1}...\n", brokerAddress, brokerPort.ToString());
        }

        protected virtual void OnConnected() // Override this method to take some actions if the connection succeeded.
        {
            //Debug.LogFormat("Connected to {0}:{1}...\n", brokerAddress, brokerPort.ToString());

            SubscribeTopics();

            if (ConnectionSucceeded != null)
            {
                ConnectionSucceeded();
            }
        }
        
        protected virtual void OnConnectionFailed(string errorMessage) // Override this method to take some actions if the connection failed.
        {
            Debug.LogWarning("Connection failed.");
            if (ConnectionFailed != null)
            {
                ConnectionFailed();
            }
        }

        protected virtual void SubscribeTopics()// Override this method to subscribe to MQTT topics.
        {
        }

        protected virtual void UnsubscribeTopics() // Override this method to unsubscribe to MQTT topics (they should be the same you subscribed to with SubscribeTopics() ).
        {
        }
        protected virtual void OnApplicationQuit() // Disconnect before the application quits.
        {
            CloseConnection();
        }
        
        protected virtual void Awake() // Initialize MQTT message queue. Remember to call base.Awake() if you override this method.
        {
            frontMessageQueue = messageQueue1;
            backMessageQueue = messageQueue2;
        }

        protected virtual void Start()  // Connect on startup if autoConnect is set to true.
        {
            if (autoConnect)
            {
                Connect();
            }
        }
        protected virtual void DecodeMessage(string topic, byte[] message)  // Override this method for each received message you need to process.
        {
            Debug.LogFormat("Message received on topic: {0}", topic);
        }

        protected virtual void OnDisconnected()  // Override this method to take some actions when disconnected.
        {
            Debug.Log("Disconnected.");
        }

        protected virtual void OnConnectionLost() // Override this method to take some actions when the connection is closed.
        {
            Debug.LogWarning("CONNECTION LOST!");
        }

        protected virtual void Update() // Processing of income messages and events is postponed here in the main thread. Remember to call ProcessMqttEvents() in Update() method if you override it.
        {
            ProcessMqttEvents();
        }

        protected virtual void ProcessMqttEvents()
        {
            // process messages in the main queue
            SwapMqttMessageQueues();
            ProcessMqttMessageBackgroundQueue();
            // process messages income in the meanwhile
            SwapMqttMessageQueues();
            ProcessMqttMessageBackgroundQueue();

            if (mqttClientConnectionClosed)
            {
                mqttClientConnectionClosed = false;
                OnConnectionLost();
            }
        }

        private void ProcessMqttMessageBackgroundQueue()
        {
            foreach (MqttMsgPublishEventArgs msg in backMessageQueue)
            {
                DecodeMessage(msg.Topic, msg.Message);
            }
            backMessageQueue.Clear();
        }

        // Swap the message queues to continue receiving message when processing a queue.
        private void SwapMqttMessageQueues()
        {
            frontMessageQueue = frontMessageQueue == messageQueue1 ? messageQueue2 : messageQueue1;
            backMessageQueue = backMessageQueue == messageQueue1 ? messageQueue2 : messageQueue1;
        }

        private void OnMqttMessageReceived(object sender, MqttMsgPublishEventArgs msg)
        {
            frontMessageQueue.Add(msg);
        }

        private void OnMqttConnectionClosed(object sender, EventArgs e)
        {
            // Set unexpected connection closed only if connected (avoid event handling in case of controlled disconnection)
            mqttClientConnectionClosed = mqttClientConnected;
            mqttClientConnected = false;
        }

        
        // Connects to the broker using the current settings.
        // <returns>The execution is done in a coroutine.</returns>
        private IEnumerator DoConnect()
        {
            // wait for the given delay
            yield return new WaitForSecondsRealtime(connectionDelay / 1000f);
            // leave some time to Unity to refresh the UI
            yield return new WaitForEndOfFrame();

            // create client instance 
            if (client == null)
            {
                try
                {
                    client = new MqttClient(brokerAddress, brokerPort, isEncrypted, null, null, isEncrypted ? MqttSslProtocols.TLSv1_2 : MqttSslProtocols.None); //isEncrypted ? MqttSslProtocols.TLSv1_2 : MqttSslProtocols.None 
                }
                catch (Exception e)
                {
                    client = null;
                    Debug.LogErrorFormat("CONNECTION FAILED! {0}", e.ToString());
                    OnConnectionFailed(e.Message);
                    yield break;
                }
            }
            else if (client.IsConnected)
            {
                yield break;
            }
            OnConnecting();

            // leave some time to Unity to refresh the UI
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            client.Settings.TimeoutOnConnection = timeoutOnConnection;
            string clientId = Guid.NewGuid().ToString();
            try
            {
                client.Connect(clientId, mqttUserName, mqttPassword);
            }
            catch (Exception e)
            {
                client = null;
                Debug.LogErrorFormat("Failed to connect to {0}:{1}:\n{2}", brokerAddress, brokerPort, e.ToString());
                OnConnectionFailed(e.Message);
                yield break;
            }
            if (client.IsConnected)
            {
                client.ConnectionClosed += OnMqttConnectionClosed;
                // register to message received 
                client.MqttMsgPublishReceived += OnMqttMessageReceived;
                mqttClientConnected = true;
                OnConnected();
            }
            else
            {
                OnConnectionFailed("CONNECTION FAILED!");
            }
        }

        private IEnumerator DoDisconnect()
        {
            yield return new WaitForEndOfFrame();
            CloseConnection();
            OnDisconnected();
        }

        private void CloseConnection()
        {
            mqttClientConnected = false;
            if (client != null)
            {
                if (client.IsConnected)
                {
                    UnsubscribeTopics();
                    client.Disconnect();
                }
                client.MqttMsgPublishReceived -= OnMqttMessageReceived;
                client.ConnectionClosed -= OnMqttConnectionClosed;
                client = null;
            }
        }
    }
}
