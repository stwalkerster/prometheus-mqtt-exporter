namespace PrometheusMqttBridge
{
    using Prometheus;
    using PrometheusMqttBridge.Config;

    public class CounterValueMetric : Metric
    {
        private readonly Counter metric;

        public CounterValueMetric(MetricConfig config) : base(config)
        {
            this.metric = Metrics.CreateCounter(config.Metric, config.Help, new CounterConfiguration
            {
                LabelNames = config.Labels?.ToArray() ?? new string[0],
                SuppressInitialValue = true
            });

            this.IncrementBy = config.IncrementByValue;
        }

        public bool IncrementBy { get; }
        
        protected override void UpdateMetric(string[] labelValues, double targetValue)
        {
            if (this.IncrementBy)
            {
                if (labelValues == null)
                {
                    this.metric.Inc(targetValue);
                }
                else
                {
                    this.metric.WithLabels(labelValues).Inc(targetValue);
                }
                
                return;
            }
            
            if (labelValues == null)
            {
                this.metric.IncTo(targetValue);
            }
            else
            {
                this.metric.WithLabels(labelValues).IncTo(targetValue);
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