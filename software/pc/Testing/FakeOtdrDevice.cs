using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OTDR.Core.Interfaces;
using OTDR.Core.Models.Acquisition;
using OTDR.Core.Models.Connections;

namespace OTDR.Core.Services.Devices;

public class FakeOtdrDevice : IOtdrDevice
{
    private readonly Random _random = new();

    public bool IsConnected { get; private set; }
    public bool IsAcquiring { get; private set; }

    public TraceData? LatestTrace { get; private set; } = new()
    {
        DistanceKm = Array.Empty<double>(),
        SignalDbm = Array.Empty<double>(),
    };

    public event EventHandler<TraceData>? TraceReceived;
    public event EventHandler<Exception>? AcquisitionFaulted { add {} remove {} }

    public async Task ConnectAsync(DeviceEndpoint endpoint)
    {
        await Task.Delay(1000);
        IsConnected = true;
    }

    public void Disconnect()
    {
        IsConnected = false;
        IsAcquiring = false;
    }

    public Task<TraceData> AcquireTraceAsync(AcquisitionSettings settings)
    {
        var trace = GenerateFakeTrace(settings);
        LatestTrace = trace;
        TraceReceived?.Invoke(this, trace);
        return Task.FromResult(trace);
    }

    public async Task StartLiveAcquisitionAsync(
        AcquisitionSettings settings,
        CancellationToken cancellationToken)
    {
        IsAcquiring = true;
        try
        {
            // Trigger single measurement irl - device does not care how many traces we want - it just produces them
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

    public void StopLiveAcquisition() => IsAcquiring = false;

    private TraceData GenerateFakeTrace(AcquisitionSettings settings)
    {
        double noise = settings.OnTimeNs / 20;
        var distance = Enumerable.Range(0, 1000)
            .Select(i => i * 0.01) // 0–10 km
            .ToArray();

        var signal = distance
            .Select(d =>
                -0.22 * d                      // attenuation
                - (d > 2.5 ? 3.0 : 0.0)        // splice
                - (d > 8.0 ? 12.0 : 0.0)       // fiber end
                + (_random.NextDouble() * noise - noise/2) * 0.1)
            .ToArray();

        return new TraceData
        {
            DistanceKm = distance,
            SignalDbm = signal,
            Settings = settings,
            AcquiredAt = DateTimeOffset.UtcNow
        };
    }
}