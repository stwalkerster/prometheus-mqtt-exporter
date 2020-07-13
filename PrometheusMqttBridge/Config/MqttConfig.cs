namespace PrometheusMqttBridge.Config
{
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class MqttConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool Tls { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }
        public List<string> Topics { get; set; }
        
        public bool UseWill { get; set; }
        public string WillTopic { get; set; }
        public bool WillRetain { get; set; }
        public string WillMessage { get; set; }
        public string WillInitialiseMessage { get; set; }
    }
}