namespace PrometheusMqttBridge
{
    using Prometheus;
    using PrometheusMqttBridge.Config;

    public class BasicCounterMetric : Metric
    {
        private readonly Counter metric;

        public BasicCounterMetric(MetricConfig config) : base(config)
        {
            this.metric = Metrics.CreateCounter(config.Metric, config.Help, config.Labels?.ToArray() ?? new string[0]);
        }

        protected override void UpdateMetric(string[] labelValues, double targetValue)
        {
            if (labelValues == null)
            {
                this.metric.Inc();
            }
            else
            {
                this.metric.WithLabels(labelValues).Inc();
            }
        }
    }
}