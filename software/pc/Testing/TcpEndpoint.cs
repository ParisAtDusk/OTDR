namespace OTDR.Core.Models.Connections;
 
public sealed record TcpEndpoint(string Host, int Port) : DeviceEndpoint($"{Host}:{Port}");