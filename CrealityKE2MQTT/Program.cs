using System.ServiceProcess;

namespace CrealityKE2MQTT;

internal static class Program
{
    public static void Main()
    {
        ServiceBase.Run([new Service()]);
    }
}
