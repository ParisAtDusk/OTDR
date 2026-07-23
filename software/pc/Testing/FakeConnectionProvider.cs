using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using OTDR.Core.Interfaces;
using OTDR.Core.Models.Connections;

namespace OTDR.Core.Services.Connections;

public class FakeConnectionProvider : IConnectionProvider
{
    public string Name => "Fake";
    private string[] ports = ["Port 1", "Port 2"];

    public IEnumerable<DeviceEndpoint> GetConnections()
    {
        foreach (var portName in ports)
            yield return new FakeEndpoint(portName);
    }
}