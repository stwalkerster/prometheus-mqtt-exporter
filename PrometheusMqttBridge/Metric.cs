namespace PrometheusMqttBridge
{
    using System.Text.RegularExpressions;
    using PrometheusMqttBridge.Config;

    public abstract class Metric
    {
        private readonly Regex regex;
        private readonly string[] labelNames;
        private readonly string munge;

        protected Metric(MetricConfig config)
        {
            this.regex = new Regex(config.Parse);
            this.labelNames = config.Labels?.ToArray();
            this.munge = config.Munge;
        }

        public void Ingest(string topic, string message)
        {
            var match = this.regex.Match(topic);
            if (!match.Success)
            {
                return;
            }
            
            var value = double.Parse(message);

            switch (this.munge)
            {
                case "div100":
                    value = value / 100;
                    break;
            }

            string[] labelValues = null;
            if (this.labelNames != null) {
                labelValues = new string[this.labelNames.Length];
                for (var i = 0; i < this.labelNames.Length; i++)
                {
                    labelValues[i] = match.Groups[this.labelNames[i]].Value;
                }
            }
            
            this.UpdateMetric(labelValues, value);
        }

        protected abstract void UpdateMetric(string[] labelValues, double targetValue);
    }
}