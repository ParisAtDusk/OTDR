using OTDR.Core.Interfaces;
using OTDR.Core.Models.Acquisition;
using System;
using System.Collections.Generic;

public class TraceAverage : ITraceAverage
{
    private readonly Queue<TraceData> _traces = new();
    private readonly int _windowSize;

    private double[]? _signalSum;

    public TraceAverage(int windowSize = 16)
    {
        if (windowSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(windowSize));

        _windowSize = windowSize;
    }

    public void Reset()
    {
        _traces.Clear();
        _signalSum = null;
    }

    public bool Add(TraceData trace)
    {
        if (trace.SignalDbm == null || trace.SignalDbm.Length == 0)
            return false;

        if (_signalSum == null)
            _signalSum = new double[trace.SignalDbm.Length];

        if (trace.SignalDbm.Length != _signalSum.Length)
            throw new ArgumentException("Trace length mismatch.");

        for (int i = 0; i < trace.SignalDbm.Length; i++)
            _signalSum[i] += trace.SignalDbm[i];

        _traces.Enqueue(trace);

        if (_traces.Count > _windowSize)
        {
            var oldTrace = _traces.Dequeue();

            for (int i = 0; i < oldTrace.SignalDbm.Length; i++)
                _signalSum[i] -= oldTrace.SignalDbm[i];
        }

        return true;
    }

    public TraceData GetResult()
    {
        if (_traces.Count == 0 || _signalSum == null)
            throw new InvalidOperationException("No traces available.");

        var latest = _traces.Peek();

        double[] average = new double[_signalSum.Length];

        for (int i = 0; i < _signalSum.Length; i++)
            average[i] = _signalSum[i] / _traces.Count;

        var newest = GetLatestTrace();

        return new TraceData
        {
            DistanceKm = newest.DistanceKm,
            AcquiredAt = DateTimeOffset.Now,
            SignalDbm = average,
            Settings = newest.Settings
        };
    }

    private TraceData GetLatestTrace()
    {
        TraceData? latest = null;

        foreach (var trace in _traces)
            latest = trace;

        return latest!;
    }
}