using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OTDR.Core.Interfaces;
using OTDR.Core.Models.Connections;

namespace OTDR.Core.Services.Connections;

public class ConnectionManager : IConnectionManager
{
    private readonly IReadOnlyList<IConnectionProvider> _providers;

    public IReadOnlyList<IConnectionProvider> Providers => _providers;

    public ConnectionManager(IEnumerable<IConnectionProvider> providers)
    {
        _providers = providers.ToList();
    }

    public Task<IReadOnlyList<DeviceEndpoint>> DiscoverAsync()
    {
        var endpoints = new List<DeviceEndpoint>();

        foreach (var provider in _providers)
            endpoints.AddRange(provider.GetConnections());

        return Task.FromResult<IReadOnlyList<DeviceEndpoint>>(endpoints);
    }
}