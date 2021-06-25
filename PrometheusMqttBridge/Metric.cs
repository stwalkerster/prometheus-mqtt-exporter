namespace PrometheusMqttBridge
{
    using System;
    using System.Collections.Generic;
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
        private readonly Dictionary<string, string> premunge;
        private readonly Dictionary<string, string> postmunge;
        private readonly Regex willTopic;
        private readonly string willValue;
        private readonly Dictionary<string, Dictionary<string, string>> willMap;

        protected Metric(MetricConfig config)
        {
            this.regex = new Regex(config.Parse);
            this.labelNames = config.Labels?.ToArray();
            this.premunge = config.Premunge;
            this.postmunge = config.Postmunge;

            if (this.premunge == null)
            {
                this.premunge = new Dictionary<string, string>();
            }

            if (this.postmunge == null)
            {
                this.postmunge = new Dictionary<string, string>();
            }

            if (!string.IsNullOrEmpty(config.Munge))
            {
                Console.WriteLine("WARNING: Legacy munge configuration detected");
                this.premunge.Add(config.Munge, string.Empty);
                this.postmunge.Add(config.Munge, string.Empty);
            }

            if (config.WillTopic != null)
            {
                this.willTopic = new Regex(config.WillTopic);
                this.willValue = config.WillValue;
                this.willMap = config.WillMap;
            }
        }

        public void Ingest(string topic, string message)
        {
            // check received topic against will topic
            if (this.willTopic != null)
            {
                var willMatch = this.willTopic.Match(topic);
                if (willMatch.Success)
                {
                    if (message != this.willValue)
                    {
                        string[] willLabelValues = null;
                        if (this.labelNames != null)
                        {
                            willLabelValues = new string[this.labelNames.Length];
                            for (var i = 0; i < this.labelNames.Length; i++)
                            {
                                var labelName = this.labelNames[i];
                                var labelValue = willMatch.Groups[labelName].Value;

                                if (this.willMap != null
                                    && this.willMap.ContainsKey(labelName)
                                    && this.willMap[labelName].ContainsKey(labelValue))
                                {
                                    labelValue = this.willMap[labelName][labelValue];
                                }

                                willLabelValues[i] = labelValue;
                            }
                        }

                        this.RemoveMetric(willLabelValues);
                    }
                }
            }

            // check received topic against topic regex
            var match = this.regex.Match(topic);
            if (!match.Success)
            {
                return;
            }

            foreach (var munge in this.premunge)
            {
                var success = true;
                switch (munge.Key)
                {
                    case "bool2int":
                        success = Munger.BoolToInt(ref message);
                        if (!success)
                        {
                            Console.WriteLine($"ERROR: Topic {topic} received a value which could not be parsed by bool2int: '{message}'");
                            ParseFailCounter.Inc();
                            return;
                        }

                        break;
                    case "jsonpath":
                        success = Munger.JsonPath(ref message, munge.Value);
                        if (!success)
                        {
                            Console.WriteLine($"ERROR: Topic {topic} received a value which could not be parsed by jsonpath({munge.Value}): '{message}'");
                            ParseFailCounter.Inc();
                            return;
                        }

                        break;
                }
            }

            double value;
            if (!double.TryParse(message, out value))
            {
                Console.WriteLine($"ERROR: Topic {topic} received a value which could not be parsed: '{message}'");
                ParseFailCounter.Inc();
                return;
            }

            foreach (var munge in this.postmunge)
            {
                var success = true;
                switch (munge.Key)
                {
                    case "div100":
                        success = Munger.Div100(ref value);
                        if (!success)
                        {
                            Console.WriteLine($"ERROR: Topic {topic} received a value which could not be parsed by div100: '{message}'");
                            ParseFailCounter.Inc();
                            return;
                        }

                        break;
                }
            }

            string[] labelValues = null;
            if (this.labelNames != null)
            {
                labelValues = new string[this.labelNames.Length];
                for (var i = 0; i < this.labelNames.Length; i++)
                {
                    labelValues[i] = match.Groups[this.labelNames[i]].Value;
                }
            }

            this.UpdateMetric(labelValues, value);
        }

        protected abstract void UpdateMetric(string[] labelValues, double targetValue);

        protected abstract void RemoveMetric(string[] labelValues);
    }
}