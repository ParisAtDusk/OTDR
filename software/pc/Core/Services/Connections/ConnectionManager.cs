using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OTDR.Core.Interfaces;
using OTDR.Core.Models.Connections;

namespace OTDR.Core.Services.Connections;

public class ConnectionManager : IConnectionManager
{
    private readonly IReadOnlyList<IConnectionProvider> _providers;

    public IReadOnlyList<IConnectionProvider> Providers => _providers;

    // Runtime DI construction — the ONLY public constructor, so DI never has to guess.
    public ConnectionManager(IServiceProvider sp)
    {
        var keyedCheck = sp.GetRequiredService<IServiceProviderIsKeyedService>();

        _providers = Enum.GetValues<OtdrDeviceKind>()
            .Where(k => keyedCheck.IsKeyedService(typeof(IConnectionProvider), k))
            .Select(k => sp.GetRequiredKeyedService<IConnectionProvider>(k))
            .ToList();
    }

    private ConnectionManager(IReadOnlyList<IConnectionProvider> providers)
    {
        _providers = providers;
    }

    public static ConnectionManager FromProviders(IEnumerable<IConnectionProvider> providers) =>
        new(providers.ToList());

    public IEnumerable<DeviceEndpoint> GetAllConnections() =>
        _providers.SelectMany(p => p.GetConnections());

    public Task<IReadOnlyList<DeviceEndpoint>> DiscoverAsync() =>
        Task.FromResult<IReadOnlyList<DeviceEndpoint>>(GetAllConnections().ToList());
}