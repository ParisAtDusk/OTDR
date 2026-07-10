using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using OTDR.Core.Interfaces;
using OTDR.Core.Models.Connections;

namespace OTDR.Core.Services.Connections;

public class SerialConnectionProvider : IConnectionProvider
{
    public string Name => "Serial";

    public IEnumerable<DeviceEndpoint> GetConnections()
    {
        foreach (var portName in SerialPort.GetPortNames().OrderBy(p => p))
            yield return new SerialEndpoint(portName);
    }
}