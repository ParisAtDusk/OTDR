using OTDR.Core.Interfaces;

namespace OTDR.Core.Models.Connections;

public abstract record DeviceEndpoint(string DisplayName)
{
    public abstract OtdrDeviceKind Kind { get; }
}

public record SerialEndpoint(string PortName) : DeviceEndpoint(PortName)
{
    public override OtdrDeviceKind Kind => OtdrDeviceKind.Serial;
}

public record FakeEndpoint(string FakeName) : DeviceEndpoint(FakeName)
{
    public override OtdrDeviceKind Kind => OtdrDeviceKind.Fake;
}

public record TcpEndpoint(string Host, int Port)
    : DeviceEndpoint($"{Host}:{Port}")
{
    public override OtdrDeviceKind Kind => OtdrDeviceKind.Tcp;
}