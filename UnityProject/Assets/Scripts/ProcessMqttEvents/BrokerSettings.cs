using System;
using System.Xml.Serialization;

namespace M2MqttUnity
{

    [Serializable]
    [XmlType(TypeName = "broker-settings")]
    public class BrokerSettings
    {
        public string host = "localhost";
        public int port = 1883;
        public bool encrypted = false;
        public string[] alternateAddress;
    }
}

