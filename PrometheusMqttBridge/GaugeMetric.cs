namespace PrometheusMqttBridge
{
    using Prometheus;
    using PrometheusMqttBridge.Config;

    public class GaugeMetric : Metric
    {
        private readonly Gauge metric;

        public GaugeMetric(MetricConfig config) : base(config)
        {
            this.metric = Metrics.CreateGauge(config.Metric, config.Help, new GaugeConfiguration
            {
                LabelNames = config.Labels?.ToArray() ?? new string[0],
                SuppressInitialValue = true
            });
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

        protected override void RemoveMetric(string[] labelValues)
        {
            if (labelValues == null)
            {
                this.metric.Unpublish();
            }
            else
            {
                this.metric.RemoveLabelled(labelValues);    
            }
        }
    }
}