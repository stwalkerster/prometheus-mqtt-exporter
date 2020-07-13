namespace PrometheusMqttBridge
{
    using Prometheus;
    using PrometheusMqttBridge.Config;

    public class CounterValueMetric : Metric
    {
        private readonly Counter metric;

        public CounterValueMetric(MetricConfig config) : base(config)
        {
            this.metric = Metrics.CreateCounter(config.Metric, config.Help, config.Labels?.ToArray() ?? new string[0]);
        }

        protected override void UpdateMetric(string[] labelValues, double targetValue)
        {
            if (labelValues == null)
            {
                this.metric.IncTo(targetValue);
            }
            else
            {
                this.metric.WithLabels(labelValues).IncTo(targetValue);
            }
        }
    }
}