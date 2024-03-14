using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrealityKE2MQTT;

public class ServiceImpl : IDisposable
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceImpl));

    static ServiceImpl()
    {
        Log4NetConfig.ConfigureLog4Net();
    }

    private readonly MQTTClient _mqttClient;
    private readonly CrealityClient _crealityClient;
    private JObject _state = new();
    private string _lastState = "";

    public ServiceImpl()
    {
        _mqttClient = new MQTTClient();
        _crealityClient = new CrealityClient();

        _crealityClient.DataReceived += _crealityClient_DataReceived;
        _crealityClient.IsConnectedChanged += _crealityClient_IsConnectedChanged;
        _mqttClient.IsConnectedChanged += _mqttClient_IsConnectedChanged;

        Connect();
    }

    private async void _mqttClient_IsConnectedChanged(object sender, EventArgs e)
    {
        await PublishState();
    }

    private async void _crealityClient_IsConnectedChanged(object sender, EventArgs e)
    {
        Log.DebugFormat("Creality client connected: {0}", _crealityClient.IsConnected);

        _state["connected"] = _crealityClient.IsConnected;

        await PublishState();
    }

    private async void _crealityClient_DataReceived(object sender, CrealityDataReceivedEventArgs e)
    {
        if (!e.Data.StartsWith("{"))
            return;

        _state = (JObject)JObject.Parse(e.Data)["result"]!["status"]!;

        _state["connected"] = _crealityClient.IsConnected;

        await PublishState();
    }

    private async Task PublishState()
    {
        var state = _state.ToString(Formatting.None);

        if (state != _lastState && _mqttClient.IsConnected)
        {
            _lastState = state;

            await _mqttClient.PublishState(state);
        }
    }

    private async void Connect()
    {
        await _mqttClient.Connect();
    }

    public void Dispose()
    {
        _mqttClient.Dispose();
        _crealityClient.Dispose();
    }
}
