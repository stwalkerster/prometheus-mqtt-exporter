namespace PrometheusMqttBridge.Config
{
    using System.Collections.Generic;

    public class MetricConfig
    {
        public string Metric { get; set; }
        public string Help { get; set; }
        public string Parse { get; set; }
        public List<string> Labels { get; set; }
        public string Munge { get; set; }
        
        public string WillTopic { get; set; }
        public string WillValue { get; set; }
    }
}