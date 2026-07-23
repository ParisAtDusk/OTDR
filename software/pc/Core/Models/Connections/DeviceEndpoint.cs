namespace OTDR.Core.Models.Connections;

public abstract record DeviceEndpoint(string DisplayName);

public record SerialEndpoint(string PortName) : DeviceEndpoint(PortName);
public record FakeEndpoint(string FakeName) : DeviceEndpoint(FakeName);