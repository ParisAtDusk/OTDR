using OTDR.Core.Models.Connections;

namespace OTDR.Core.Interfaces;

public interface ITransportFactory
{
    IScpiTransport Create(DeviceEndpoint endpoint);
}