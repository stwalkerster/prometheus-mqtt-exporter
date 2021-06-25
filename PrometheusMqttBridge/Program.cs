using System;

namespace PrometheusMqttBridge
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Prometheus;
    using Prometheus.DotNetRuntime;
    using PrometheusMqttBridge.Config;
    using uPLibrary.Networking.M2Mqtt;
    using uPLibrary.Networking.M2Mqtt.Messages;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    class Program
    {
        private static readonly Counter MqttMessagesReceived = Metrics.CreateCounter(
            "mqttbridge_messages_total",
            "Number of MQTT messages received by bridge");
        
        private static MqttClient client;
        private static Configuration config;
        private static readonly List<Metric> metrics = new List<Metric>();

        static void Main(string[] args)
        {
            var input = new StreamReader(args[0]);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            config = deserializer.Deserialize<Configuration>(input);
            metrics.AddRange(config.Gauges.Select(x => new GaugeMetric(x)).ToList());
            metrics.AddRange(config.Counters.Select(x => new CounterValueMetric(x)));

            var metricServer = new MetricServer(config.Prometheus.Port, config.Prometheus.Path);
            if (config.Prometheus.SkipMonitoringProcess)
            {
                Metrics.SuppressDefaultMetrics();
                MqttMessagesReceived.Unpublish();
            }

            metricServer.Start();
            
            if (!config.Prometheus.SkipMonitoringProcess)
            {
                DotNetRuntimeStatsBuilder.Default().StartCollecting();
            }

            client = new MqttClient(
                config.Mqtt.Host,
                config.Mqtt.Port,
                config.Mqtt.Tls,
                MqttSslProtocols.TLSv1_2,
                (sender, certificate, chain, errors) => true,
                null);

            client.MqttMsgPublishReceived += MessageReceived;
            client.ConnectionClosed += ConnectionClosed;
            ConnectionClosed(client, EventArgs.Empty);
        }

        private static void ConnectionClosed(object sender, EventArgs e)
        {
            client.Connect(
                config.Mqtt.ClientId,
                config.Mqtt.Username,
                config.Mqtt.Password,
                config.Mqtt.WillRetain,
                MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,
                config.Mqtt.UseWill,
                config.Mqtt.WillTopic,
                config.Mqtt.WillMessage,
                true,
                10000);

            if (config.Mqtt.UseWill)
            {
                client.Publish(
                    config.Mqtt.WillTopic,
                    Encoding.UTF8.GetBytes(config.Mqtt.WillInitialiseMessage),
                    MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,
                    config.Mqtt.WillRetain);
            }

            var topics = config.Mqtt.Topics.ToArray();
            var qos = Enumerable.Repeat(MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, topics.Length).ToArray();

            client.Subscribe(topics, qos);
        }

        private static void MessageReceived(object sender, MqttMsgPublishEventArgs e)
        {
            if (! config.Prometheus.SkipMonitoringProcess)
            {
                MqttMessagesReceived.Inc();
            }

            foreach (var metric in metrics)
            {
                metric.Ingest(e.Topic, Encoding.UTF8.GetString(e.Message));
            }
        }
    }
}