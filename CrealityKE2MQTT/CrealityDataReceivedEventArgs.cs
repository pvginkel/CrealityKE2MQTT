using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrealityKE2MQTT;

internal class CrealityDataReceivedEventArgs(string data) : EventArgs
{
    public string Data { get; } = data;
}
