
using System;
namespace OTDR.Core.Models.Acquisition;
public sealed record TraceData
{
    public required double[] DistanceKm { get; init; }
    public required double[] SignalDbm { get; init; }
    public DateTimeOffset AcquiredAt { get; init; } = DateTimeOffset.UtcNow;
    public AcquisitionSettings? Settings { get; init; }
}