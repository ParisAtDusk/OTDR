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
    public bool IsAcquiring { get; private set; } = false;
    public TraceData? LatestTrace { get; private set; } = new()
    {
        DistanceKm = [],
        SignalDbm = [],
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
        // _readLoopTask = Task.Run(() => ReadLoopAsync(_stream, _readLoopCts.Token));
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
        await SendRawAsync("TRACE:DATA?");

        byte[] one = new byte[1];

        // Skip leading CR/LF
        do
        {
            await _stream!.ReadExactlyAsync(one);
        }
        while (one[0] == '\r' || one[0] == '\n');

        if (one[0] != '#')
            throw new Exception($"Expected '#', got 0x{one[0]:X2}");

        await _stream!.ReadExactlyAsync(one);
        int digits = one[0] - '0';

        if (digits < 1 || digits > 9)
            throw new Exception("Invalid SCPI block length digit count.");

        byte[] lengthBytes = new byte[digits];
        await _stream.ReadExactlyAsync(lengthBytes);

        int length = int.Parse(Encoding.ASCII.GetString(lengthBytes));

        byte[] bytes = new byte[length];
        await _stream.ReadExactlyAsync(bytes);

        int count = length / sizeof(ushort);

        TraceData data = new()
        {
            DistanceKm = new double[count],
            SignalDbm = new double[count]
        };

        for (int i = 0; i < count; i++)
        {
            data.SignalDbm[i] =
                BitConverter.ToUInt16(bytes, i * sizeof(ushort));

            data.DistanceKm[i] = i;
        }

        LatestTrace = data;
        TraceReceived?.Invoke(this, data);
        return data;
    }

     public async Task StartLiveAcquisitionAsync(
        AcquisitionSettings settings,
        CancellationToken cancellationToken)
    {
        // TODO: First send START command
        IsAcquiring = true;
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await AcquireTraceAsync(settings);

                await Task.Delay(250, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            IsAcquiring = false;
        }
    }

    public Task StopLiveAcquisitionAsync() => Task.FromResult(IsAcquiring = false); // TODO: Send STOP command

    public void Dispose() => Disconnect();
}