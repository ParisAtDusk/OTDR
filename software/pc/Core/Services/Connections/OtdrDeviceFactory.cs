using System;
using Microsoft.Extensions.DependencyInjection;
using OTDR.Core.Interfaces;
using OTDR.Core.Models.Connections;

namespace OTDR.Core.Services.Devices;

public sealed class OtdrDeviceFactory : IOtdrDeviceFactory
{
    private readonly IServiceProvider _sp;
    private readonly IServiceProviderIsKeyedService _keyedCheck;

    public OtdrDeviceFactory(IServiceProvider sp)
    {
        _sp = sp;
        _keyedCheck = sp.GetRequiredService<IServiceProviderIsKeyedService>();
    }

    public IOtdrDevice Create(DeviceEndpoint endpoint)
    {
        if (!_keyedCheck.IsKeyedService(typeof(IOtdrDevice), endpoint.Kind))
        {
            throw new NotSupportedException(
                $"No {nameof(IOtdrDevice)} registered for kind '{endpoint.Kind}'.");
        }
        return _sp.GetRequiredKeyedService<IOtdrDevice>(endpoint.Kind);
    }
}