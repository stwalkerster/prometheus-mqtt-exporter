# Prometheus MQTT Exporter

A Prometheus exporter to log values from an MQTT broker

## Running the exporter

This exporter can be run using Docker:

```
docker run -d -v ./config.yml:/config/config.yml -p 9100:9100 docker.scimonshouse.net/promqtt:latest
```

## Configuration

You will need a basic configuration file - you can use the below as a starter, and it should be just about the minimum configuration you can get away with. The configuration file must be valid YAML. It should be noted that this is not a particularly useful configuration, as it doesn't configure any metrics from MQTT.

```
mqtt:
  host: mqttbroker.example.net
  port: 8883
  tls: true
  username: prometheus
  password: supersekrit
  clientId: prometheus-exporter
  topics:
    - zigbee2mqtt/+
  useWill: false
prometheus:
  port: 9100
  path: metrics/
counters:
gauges:
```

The configuration file is split into several sections.

### `mqtt`

This section configures the connection to your MQTT broker. 
* `host` is the hostname or IP address of your broker
* `port` is the port number your broker listens on. This is usually 1883 (for non-TLS connections) or 8883 (for TLS connections)
* `tls` (optional; default false) is a boolean flag determining if the connection should be made over a secure channel
* `username` and `password` are the credentials to connect to the broker with
* `clientId` is the client name to use to connect to the broker with. This must be unique amongst all clients connecting to the broker.
* `topics` is a list of MQTT topics to subscribe to. All topics you wish to create metrics from *must* be listed here, and [use of wildcards](https://mosquitto.org/man/mqtt-7.html#idm22) (`+` and `#`) is perfectly acceptable.
* `useWill` (optional; default false) is a boolean flag determining whether or not to publish a last will and testament message on disconnection.
* `willTopic` (optional) is the topic on which to publish the last will message.
* `willMessage` (optional) is the message to publish as the last will.
* `willInitialiseMessage` (optional) is a message to send to the will topic immediately on connection - sometimes known as a birth message.
* `willRetain` (optional; default false) is a boolean flag determining whether to set the retain flag on the will messages.

### `prometheus`

This section configures the metrics exporter.
* `port` is the port number on which to publish the metrics. This must be published outside of the container.
* `path` is the path under which to publish the metrics. By default, Prometheus assumes this set to "metrics/", so that is the value we recommend here.
* `skipMonitoringProcess` (optional; default false) is a boolean flag which allows you to disable the publication of metrics about the exporter itself.

### `counters` and `gauges`

This section configures the metrics published from MQTT.

* `metric` is the name of the Prometheus metric to publish
* `help` is the help text to publish for this metric
* `parse` is a regular expression matching the MQTT topic from which to extract the value.
* `labels` is a list of label keys to publish with the metric. These must be populated by capturing groups from the `parse` regex.
* `premunge` and `postmunge` are dictionaries of munge filters to apply to the received MQTT message before publishing a metric. Premunges operate on the MQTT message as a string; postmunges operate on the value as casted to a number.
* `labelMap` is a dictionary of dictionaries, allowing label values to be remapped to other values.
* `willTopic`, `willValue`, and `willMap` are (a string, string and dictionary of dictionaries respectively) a set of values which allow metrics to be unpublished when a certain condition is met.
* `incrementByValue` is a boolean flag only valid on counters. When this is set, the counter is incremented by the value passed via MQTT, rather than incremented *to* the value passed via MQTT.

### Example configuration 1

For MQTT topics:
```
sensor/pressure = 1001.65
sensor/humidity/indoors = 45.23
sensor/humidity/outdoors = 75.29
```

And this configuration:
```
gauges:
  - metric: pressure_pascals
    help: Barometric pressure
    parse: sensor/pressure
  - metric: humidity_percent
    help: Relative humidity
    parse: sensor/humidity/(?<location>.*)
    labels:
      - location
```

Would produce the following metrics:

```
# HELP pressure_pascals Barometric pressure
# TYPE pressure_pascals gauge
pressure_pascals 1001.65
# HELP humidity_percent Relative humidity
# TYPE humidity_percent gauge
humidity_percent{location="indoors"} 45.23
humidity_percent{location="outdoors"} 75.29
```

### Example configuration 2

For MQTT topics:
```
sensor/humidity/indoors = 45.23
sensor/humidity/outdoors = 75.29
```

And this configuration:
```
gauges:
  - metric: humidity_percent
    help: Relative humidity
    parse: sensor/humidity/(?<location>.*)
    labels:
      - location
    postmunge:
      div100:
    labelMap:
      location:
        indoors: Office
        outdoors: Garden
```

Would produce the following metrics:

```
# HELP humidity_percent Relative humidity
# TYPE humidity_percent gauge
humidity_percent{location="Office"} 0.4523
humidity_percent{location="Garden"} 0.7529
```


### Example configuration 3

For MQTT topics:
```
zigbee2mqtt/temp01 = {"battery":100,"humidity":65.32,"linkquality":21,"temperature":15.7,"voltage":3200}
zigbee2mqtt/temp02 = {"battery":100,"humidity":45.10,"linkquality":96,"temperature":22.7,"voltage":3150}
```

And this configuration:
```
  - metric: temperature_celsius
    help: Temperature
    parse: zigbee2mqtt/(?<device>temp[0-9]+)$
    premunge:
        jsonpath: "$.temperature"
    labels:
      - device
```

Would produce the following metrics:
```
# HELP temperature_celsius Temperature
# TYPE temperature_celsius gauge
temperature_celsius{device="temp01"} 15.7
temperature_celsius{device="temp02"} 22.7
```

### Example configuration 4

For MQTT topics:
```
zigbee2mqtt/thingA = 3
zigbee2mqtt/thingB = 3
```

And this configuration:
```
  - metric: thing_A_total
    help: Thing
    parse: zigbee2mqtt/thingA
    incrementByValue: false
  - metric: thing_B_total
    help: Thing
    parse: zigbee2mqtt/thingB
    incrementByValue: true
```

Would produce the following metrics:
```
# HELP temperature_celsius Temperature
# TYPE temperature_celsius gauge
thing_A_total 3
thing_B_total 3
```

Then, sending these values:
```
zigbee2mqtt/thingA = 4
zigbee2mqtt/thingB = 4
```

Would produce the following metrics:
```
# HELP temperature_celsius Temperature
# TYPE temperature_celsius gauge
thing_A_total 4
thing_B_total 7
```

`thing_A_total` is incremented up to 4, whereas `thing_B_total` is incremented *by* 4.

## Munging

Available pre-munges are:
* jsonpath
* bool2int
Available post-munges are:
* div100

TODO

## Label maps
TODO

## Metric wills
TODO
