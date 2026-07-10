using System;
using OTDR.Core.Interfaces;
using OTDR.Core.Models.Connections;

namespace OTDR.Core.Services.Connections;

public class TransportFactory : ITransportFactory
{
    public IScpiTransport Create(DeviceEndpoint endpoint) => endpoint switch
    {
        SerialEndpoint s => new SerialScpiTransport(s.PortName),
        _ => throw new NotSupportedException($"No transport for endpoint type {endpoint.GetType().Name}")
    };
}