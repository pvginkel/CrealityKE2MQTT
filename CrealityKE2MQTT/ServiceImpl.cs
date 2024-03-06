using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrealityKE2MQTT;

public class ServiceImpl : IDisposable
{
    private static readonly HashSet<string> AllowedKeys =
    [
        "TotalLayer",
        "layer",
        "bedTemp0",
        "nozzleTemp",
        "printFileName",
        "printJobTime",
        "printLeftTime",
        "printProgress",
        "printStartTime",
        "targetBedTemp0",
        "targetNozzleTemp",
        "usedMaterialLength"
    ];

    private readonly MQTTClient _mqttClient;
    private readonly CrealityClient _crealityClient;
    private readonly JObject _state = new();
    private string _lastState = "";

    public ServiceImpl()
    {
        _mqttClient = new MQTTClient();
        _crealityClient = new CrealityClient();

        _crealityClient.DataReceived += _crealityClient_DataReceived;
        _crealityClient.IsConnectedChanged += _crealityClient_IsConnectedChanged;

        Connect();
    }

    private async void _crealityClient_IsConnectedChanged(object sender, EventArgs e)
    {
        _state["connected"] = _crealityClient.IsConnected;

        await PublishState();
    }

    private async void _crealityClient_DataReceived(object sender, CrealityDataReceivedEventArgs e)
    {
        if (!e.Data.StartsWith("{"))
            return;

        var obj = JObject.Parse(e.Data);

        foreach (var entry in obj)
        {
            if (AllowedKeys.Contains(entry.Key))
                _state[entry.Key] = entry.Value;
        }

        await PublishState();
    }

    private async Task PublishState()
    {
        var state = _state.ToString(Formatting.None);

        if (state != _lastState)
        {
            _lastState = state;

            await _mqttClient.PublishState(state);
        }
    }

    private async void Connect()
    {
        await _mqttClient.Connect();
        await _crealityClient.Connect();
    }

    public void Dispose()
    {
        _mqttClient.Dispose();
        _crealityClient.Dispose();
    }
}
