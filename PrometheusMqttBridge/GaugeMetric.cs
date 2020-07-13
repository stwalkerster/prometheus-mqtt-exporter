namespace PrometheusMqttBridge
{
    using Prometheus;
    using PrometheusMqttBridge.Config;

    public class GaugeMetric : Metric
    {
        private readonly Gauge metric;

        public GaugeMetric(MetricConfig config) : base(config)
        {
            this.metric = Metrics.CreateGauge(config.Metric, config.Help, config.Labels?.ToArray() ?? new string[0]);
        }

        protected override void UpdateMetric(string[] labelValues, double targetValue)
        {
            if (labelValues == null)
            {
                this.metric.Set(targetValue);
            }
            else
            {
                this.metric.WithLabels(labelValues).Set(targetValue);
            }
        }
    }
}