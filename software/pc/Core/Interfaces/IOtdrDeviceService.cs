using System;
using System.Threading;
using System.Threading.Tasks;
using OTDR.Core.Models.Acquisition;
using OTDR.Core.Models.Connections;

namespace OTDR.Core.Interfaces;

public interface IOtdrDevice
{
    Task ConnectAsync(DeviceEndpoint endpoint);
    void Disconnect();
    bool IsConnected { get; }

    Task<TraceData> AcquireTraceAsync(AcquisitionSettings settings);

    Task StartLiveAcquisitionAsync(AcquisitionSettings settings, CancellationToken cancellationToken);
    void StopLiveAcquisition();
    bool IsAcquiring { get; }

    TraceData? LatestTrace { get; }

    event EventHandler<TraceData> TraceReceived;
    event EventHandler<Exception>? AcquisitionFaulted;
}