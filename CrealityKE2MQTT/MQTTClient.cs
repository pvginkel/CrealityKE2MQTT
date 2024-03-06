using System.Configuration;
using System.Text.RegularExpressions;
using log4net;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrealityKE2MQTT;

internal class MQTTClient : IDisposable
{
    private const string TopicPrefix = "creality";
    private static readonly ILog Log = LogManager.GetLogger(typeof(MQTTClient));
    private static readonly TimeSpan ReconnectInterval = TimeSpan.FromSeconds(15);

    private readonly MqttFactory _factory = new();
    private readonly IMqttClient _client;
    private readonly string _entityTopic;
    private readonly string _stateTopic;
    private volatile bool _closed;

    private static string SafeName(string name) => Regex.Replace(name, "[^a-zA-Z0-9_-]", "_");

    public MQTTClient()
    {
        _entityTopic = $"{TopicPrefix}/{ConfigurationManager.AppSettings["PrinterName"]}";
        _stateTopic = $"{_entityTopic}/state";

        _client = _factory.CreateMqttClient();

        _client.DisconnectedAsync += _client_DisconnectedAsync;
    }

    private async Task _client_DisconnectedAsync(MqttClientDisconnectedEventArgs e)
    {
        if (!e.ClientWasConnected)
            return;

        Log.Info("Reconnecting");

        await Connect();
    }

    public async Task Connect()
    {
        Log.Info("Connecting");

        while (!_closed)
        {
            try
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(ConfigurationManager.AppSettings["MQTTServer"])
                    .WithCredentials(
                        "mqtt",
                        Environment.ExpandEnvironmentVariables(
                            ConfigurationManager.AppSettings["MQTTPassword"]
                        )
                    )
                    .WithWillTopic(_stateTopic)
                    .WithWillPayload("offline")
                    .WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithWillRetain()
                    .Build();

                await _client.ConnectAsync(mqttClientOptions);

                Log.Info("Connected");

                await PublishDiscovery();
                await SetOnline();

                break;
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to connect, retrying", ex);

                await Task.Delay(ReconnectInterval);
            }
        }
    }

    public async Task PublishState(string data)
    {
        try
        {
            await _client.PublishStringAsync(
                _entityTopic,
                data,
                MqttQualityOfServiceLevel.AtLeastOnce
            );
        }
        catch (Exception ex)
        {
            Log.Error("Failed to publish state", ex);
        }
    }

    private async Task SetOnline()
    {
        await _client.PublishStringAsync(
            _stateTopic,
            "online",
            MqttQualityOfServiceLevel.AtLeastOnce
        );
    }

    private async Task PublishDiscovery()
    {
        var uri = new Uri(ConfigurationManager.AppSettings["PrinterURL"]);
        var uniqueIdentifier = $"creality-ender-3-v3-ke_{SafeName(uri.Host)}";

        var message = new JObject
        {
            ["availability"] = new JArray(new JObject { ["topic"] = _stateTopic }),
            ["device"] = new JObject
            {
                ["identifiers"] = new JArray(uniqueIdentifier),
                ["manufacturer"] = "Pieter",
                ["model"] = "Creality Ender-3 V3 KE",
                ["name"] = ConfigurationManager.AppSettings["PrinterName"]
            },
            ["device_class"] = JValue.CreateNull(),
            ["json_attributes_topic"] = _entityTopic,
            ["name"] = JValue.CreateNull(),
            ["object_id"] = ConfigurationManager.AppSettings["PrinterEntityID"],
            ["state_topic"] = _entityTopic,
            ["unit_of_measurement"] = "%",
            ["value_template"] = "{{ value_json.printProgress }}",
            ["unique_id"] = uniqueIdentifier
        };

        await _client.PublishStringAsync(
            $"homeassistant/sensor/{uniqueIdentifier}/config",
            message.ToString(Formatting.None),
            qualityOfServiceLevel: MqttQualityOfServiceLevel.AtLeastOnce
        );
    }

    public void Dispose()
    {
        _closed = true;

        _client.Dispose();
    }
}
