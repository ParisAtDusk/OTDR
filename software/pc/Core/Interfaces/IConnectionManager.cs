using System.Collections.Generic;
using System.Threading.Tasks;
using OTDR.Core.Models.Connections;

namespace OTDR.Core.Interfaces;

public interface IConnectionManager
{
    IReadOnlyList<IConnectionProvider> Providers { get; }
    Task<IReadOnlyList<DeviceEndpoint>> DiscoverAsync();
}