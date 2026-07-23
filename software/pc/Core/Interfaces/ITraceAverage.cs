using OTDR.Core.Models.Acquisition;

namespace OTDR.Core.Interfaces;
public interface ITraceAverage
{
    public void Reset();
    public bool Add(TraceData trace);
    public TraceData GetResult();
}