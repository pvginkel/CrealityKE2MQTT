using System.ServiceProcess;

namespace CrealityKE2MQTT;

internal class Service : ServiceBase
{
    private ServiceImpl? _service;

    protected override void OnStart(string[] args)
    {
        _service = new ServiceImpl();
    }

    protected override void OnStop()
    {
        _service?.Dispose();
        _service = null;
    }
}
