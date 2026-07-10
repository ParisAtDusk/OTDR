namespace OTDR.Core.Models.Connections;

public abstract record DeviceEndpoint(string DisplayName);

public record SerialEndpoint(string PortName) : DeviceEndpoint(PortName);