using System.Collections.Generic;
using OTDR.Core.Models.Connections;

namespace OTDR.Core.Interfaces;

public interface IConnectionProvider
{
    string Name { get; }
    IEnumerable<DeviceEndpoint> GetConnections();
}