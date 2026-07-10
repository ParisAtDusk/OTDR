using System.Collections.Generic;
using OTDR.Core.Interfaces;

namespace OTDR.Core.Services.Connections;

public class ConnectionManager
{
    public List<IConnectionProvider> Providers { get; } = new();

    public ConnectionManager()
    {
        Providers.Add(new SerialConnectionProvider());
        // Providers.Add(new EthernetConnectionProvider()); // Maybe in the future
    }
}