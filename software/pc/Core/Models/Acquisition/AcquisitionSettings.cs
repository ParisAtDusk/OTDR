namespace OTDR.Core.Models.Acquisition;
public sealed record AcquisitionSettings
{
    public double OnTimeNs { get; set; } = 1000.0;
    // public int SoftwareAveragingCount { get; set; } = 1;
    public int TimeDomainOversampling { get; set; } = 1;
}