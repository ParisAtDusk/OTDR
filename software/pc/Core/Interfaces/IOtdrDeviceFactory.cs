using OTDR.Core.Models.Connections;

namespace OTDR.Core.Interfaces;

public interface IOtdrDeviceFactory
{
    IOtdrDevice Create(DeviceEndpoint endpoint);
}