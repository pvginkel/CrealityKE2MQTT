using System.Configuration;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrealityKE2MQTT;

internal class CrealityClient : IDisposable
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(CrealityClient));
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan TimeoutInterval = TimeSpan.FromSeconds(3);

    private readonly CancellationTokenSource _cts = new();
    private readonly ManualResetEventSlim _event = new();
    private readonly HttpClient _client = new();
    private volatile int _connected;

    public bool IsConnected => _connected != 0;

    public event EventHandler<CrealityDataReceivedEventArgs>? DataReceived;
    public event EventHandler? IsConnectedChanged;

    public CrealityClient()
    {
        _client.Timeout = TimeoutInterval;

        PollLoop();
    }

    private async void PollLoop()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                using var response = await _client.GetAsync(
                    "http://192.168.178.107:7125/printer/objects/query?heater_bed&extruder&print_stats&display_status",
                    _cts.Token
                );

                SetConnected(true);

                OnDataReceived(
                    new CrealityDataReceivedEventArgs(await response.Content.ReadAsStringAsync())
                );
            }
            catch (Exception ex)
            {
                SetConnected(false);

                Log.Warn("Failed to get status", ex);
            }

            await Task.Delay(Interval, _cts.Token);
        }

        _event.Set();
    }

    public void Dispose()
    {
        _cts.Cancel();

        _event.Wait();

        _client.Dispose();

        SetConnected(false);

        OnIsConnectedChanged();
    }

    private void SetConnected(bool connected)
    {
        var expected = connected ? 0 : 1;
        var target = connected ? 1 : 0;

        if (Interlocked.CompareExchange(ref _connected, target, expected) == expected)
            OnIsConnectedChanged();
    }

    protected virtual void OnDataReceived(CrealityDataReceivedEventArgs e)
    {
        DataReceived?.Invoke(this, e);
    }

    protected virtual void OnIsConnectedChanged()
    {
        IsConnectedChanged?.Invoke(this, EventArgs.Empty);
    }
}
