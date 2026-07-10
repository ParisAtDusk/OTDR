using System.IO.Ports;
using OTDR.Core.Interfaces;

namespace OTDR.Core.Services.Connections;

public class SerialScpiTransport : IScpiTransport
{
    private readonly SerialPort _port;

    public SerialScpiTransport(string portName, int baudRate = 9600)
    {
        _port = new SerialPort(portName, baudRate)
        {
            NewLine = "\n",
            ReadTimeout = 2000,
            WriteTimeout = 2000
        };
    }

    public bool IsOpen => _port.IsOpen;

    public void Connect() => _port.Open();

    public void Disconnect()
    {
        if (_port.IsOpen)
            _port.Close();
    }

    public void Write(string command) => _port.WriteLine(command);

    public string Query(string command)
    {
        _port.WriteLine(command);
        return _port.ReadLine();
    }

    public void Dispose() => Disconnect();
}