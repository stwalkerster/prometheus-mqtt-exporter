namespace PrometheusMqttBridge.Config
{
    using System.Collections.Generic;

    public class MetricConfig
    {
        public string Metric { get; set; }
        public string Help { get; set; }
        public string Parse { get; set; }
        public List<string> Labels { get; set; }
        public Dictionary<string, string> Premunge { get; set; }
        public Dictionary<string, string> Postmunge { get; set; }
        public string WillTopic { get; set; }
        public string WillValue { get; set; }
        public Dictionary<string, Dictionary<string, string>> WillMap { get; set; }
        
        public Dictionary<string, Dictionary<string, string>> LabelMap { get; set; }
        
        // legacy settings
        public string Munge { get; set; }
    }
}