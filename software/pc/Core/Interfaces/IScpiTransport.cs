using System;

namespace OTDR.Core.Interfaces;

public interface IScpiTransport : IDisposable
{
    bool IsOpen { get; }
    void Connect();
    void Disconnect();
    void Write(string command);
    string Query(string command);
}