using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using OTDR.Core.Interfaces;
using OTDR.Core.Models.Connections;

namespace OTDR.Core.Services.Connections;

public sealed class TcpConnectionProvider : IConnectionProvider
{
    private readonly string _host;
    private readonly int _port;
    private readonly TimeSpan _probeTimeout;

    public string Name => "TCP";

    public TcpConnectionProvider(
        string host = "127.0.0.1",
        int port = 2137,
        TimeSpan? probeTimeout = null)
    {
        _host = host;
        _port = port;
        _probeTimeout = probeTimeout ?? TimeSpan.FromMilliseconds(300);
    }

    public IEnumerable<DeviceEndpoint> GetConnections()
    {
        if (IsReachable(_host, _port, _probeTimeout))
        {
            yield return new TcpEndpoint(_host, _port);
        }
    }
    private static bool IsReachable(string host, int port, TimeSpan timeout)
    {
        using var client = new TcpClient();
        using var cts = new CancellationTokenSource(timeout);

        try
        {
            client.ConnectAsync(host, port, cts.Token)
                  .AsTask()
                  .GetAwaiter()
                  .GetResult();

            return client.Connected;
        }
        catch (OperationCanceledException)
        {
            // Probe timed out - treat as unreachable
            return false;
        }
        catch (SocketException)
        {
            // Refused, host unreachable, etc. - treat as unreachable
            return false;
        }
    }
}