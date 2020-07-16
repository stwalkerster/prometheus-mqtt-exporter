namespace PrometheusMqttBridge
{
    using System;
    using System.Text.RegularExpressions;
    using Prometheus;
    using PrometheusMqttBridge.Config;

    public abstract class Metric
    {
        private static readonly Counter ParseFailCounter = Metrics.CreateCounter(
            "mqttbridge_parse_failures_total",
            "Number of parse failures in the MQTT messages");
        
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

            switch (this.munge)
            {
                case "bool2int":
                    switch (message.Trim().ToLowerInvariant())
                    {
                        case "on":
                        case "true":
                        case "1":
                            message = "1";
                            break;
                        case "off":
                        case "false":
                        case "0":
                            message = "0";
                            break;
                        default:
                            Console.WriteLine($"ERROR: Topic {topic} received a value which could not be parsed by bool2int: '{message}'");
                            ParseFailCounter.Inc();
                            return;
                    }
                    break;
            }
            
            double value;
            if (!double.TryParse(message, out value))
            {
                Console.WriteLine($"ERROR: Topic {topic} received a value which could not be parsed: '{message}'");
                ParseFailCounter.Inc();
                return;
            }

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