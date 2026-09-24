using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OTDR.Core.Interfaces;
using OTDR.Core.Models.Acquisition;
using OTDR.Core.Models.Connections;

namespace OTDR.Core.Services.Connections;

public sealed class TcpOtdrDevice : IOtdrDevice, IDisposable
{
    private TcpClient? _client;
    private NetworkStream? _stream;
    private CancellationTokenSource? _readLoopCts;
    private Task? _readLoopTask;

    public bool IsConnected => _client?.Connected ?? false;
    public bool IsAcquiring => false;
        public TraceData? LatestTrace { get; private set; } = new()
    {
        DistanceKm = Array.Empty<double>(),
        SignalDbm = Array.Empty<double>(),
    };

    public event EventHandler<TraceData> TraceReceived = delegate { };
    public event EventHandler<Exception>? AcquisitionFaulted;

    public event EventHandler<string>? RawLineReceived;

    public async Task ConnectAsync(DeviceEndpoint endpoint)
    {
        if (endpoint is not TcpEndpoint tcp)
        {
            throw new ArgumentException(
                $"{nameof(TcpOtdrDevice)} only supports {nameof(TcpEndpoint)}, got {endpoint.GetType().Name}.",
                nameof(endpoint));
        }

        _client = new TcpClient();
        await _client.ConnectAsync(tcp.Host, tcp.Port).ConfigureAwait(false);
        _stream = _client.GetStream();

        _readLoopCts = new CancellationTokenSource();
        _readLoopTask = Task.Run(() => ReadLoopAsync(_stream, _readLoopCts.Token));
    }

    public void Disconnect()
    {
        _readLoopCts?.Cancel();

        try
        {
            _stream?.Dispose();
            _client?.Dispose();
        }
        finally
        {
            _stream = null;
            _client = null;
            _readLoopCts = null;
            _readLoopTask = null;
        }
    }

    public async Task SendRawAsync(string text, CancellationToken cancellationToken = default)
    {
        if (_stream is null)
        {
            throw new InvalidOperationException("Not connected.");
        }

        byte[] bytes = Encoding.ASCII.GetBytes(text + "\n");
        await _stream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
    }

    private async Task ReadLoopAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                string? line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (line is null)
                {
                    break; // peer closed the connection
                }
                else
                {
                    System.Console.WriteLine(line);
                    
                }

                RawLineReceived?.Invoke(this, line);
            }
        }
        catch (ObjectDisposedException)
        {
            // expected when disconnect tears down the stream mid-read
        }
        catch (IOException)
        {
            // connection dropped from underneath
        }
    }

    public async Task<TraceData> AcquireTraceAsync(AcquisitionSettings settings)
    {
        TraceData data = new TraceData
        {
            DistanceKm = [0.0],
            SignalDbm = [0.0],
        };
        await SendRawAsync("*IDN?\n");
        LatestTrace = data;
        TraceReceived?.Invoke(this, data);
        return data;
    }

    public Task StartLiveAcquisitionAsync(AcquisitionSettings settings, CancellationToken cancellationToken)
    {
        return SendRawAsync("TRACE:DATA?\n");;
    }

    public Task StopLiveAcquisitionAsync() => SendRawAsync("ACQ:PARAM:PUL:WID?\n");

    public void Dispose() => Disconnect();
}