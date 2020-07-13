namespace PrometheusMqttBridge.Config
{
    using System.Collections.Generic;

    public class Configuration
    {
        private List<MetricConfig> gauges;
        private List<MetricConfig> counters;
        public MqttConfig Mqtt { get; set; }
        public PrometheusConfig Prometheus { get; set; }

        public List<MetricConfig> Gauges
        {
            get => this.gauges ?? new List<MetricConfig>();
            set => this.gauges = value;
        }

        public List<MetricConfig> Counters
        {
            get => this.counters ?? new List<MetricConfig>();
            set => this.counters = value;
        }
    }
}