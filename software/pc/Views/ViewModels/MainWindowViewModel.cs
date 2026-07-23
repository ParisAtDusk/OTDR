using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OTDR.Core.Interfaces;
using OTDR.Core.Models.Acquisition;
using OTDR.Core.Models.Connections;
using OTDR.Core.Services.Connections;

namespace OTDR.Views.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IOtdrDevice _device;
    private readonly IConnectionManager _connectionManager;
    private CancellationTokenSource? _liveAcquisitionCts;

    private DateTimeOffset _lastUiUpdate = DateTimeOffset.MinValue;
    private static readonly TimeSpan MinUiInterval = TimeSpan.FromMilliseconds(100);

    public IReadOnlyList<IConnectionProvider> ConnectionProviders => _connectionManager.Providers;

    // Connection state
    public enum ConnectionStatus_e
    {
        Connected,
        Connecting,
        Disconnected,
    }

    [ObservableProperty]
    private DeviceEndpoint? selectedEndpoint;

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConnectionStatusString))]
    [NotifyPropertyChangedFor(nameof(StatusBrushKey))]
    private ConnectionStatus_e connectionStatus = ConnectionStatus_e.Disconnected;
    public string ConnectionStatusString => ConnectionStatus switch
    {
        ConnectionStatus_e.Connected => "Connected",
        ConnectionStatus_e.Disconnected => "Disconnected",
        ConnectionStatus_e.Connecting => "Connecting...",
        _ => "Unknown"
    };
    public string StatusBrushKey => ConnectionStatus switch
    {
        ConnectionStatus_e.Connected => "StatusConnectedBrush",
        ConnectionStatus_e.Connecting => "StatusBusyBrush",
        ConnectionStatus_e.Disconnected => "StatusDisconnectedBrush",
        _ => "StatusDisconnectedBrush"
    };

    // Properties

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartLiveAcquisitionCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelLiveAcquisitionCommand))]
    private bool isLiveAcquiring;

    [ObservableProperty]
    private TraceData? currentTrace;

    [ObservableProperty]
    private double lineWidth = 1.0;

    [ObservableProperty]
    private bool showMarkers;

    [ObservableProperty]
    private bool showLegend;

    [ObservableProperty]
    private bool showGrid = true;
    [ObservableProperty]
    private int? softwareAveraging = 1;
    [ObservableProperty]
    private int measurementProgress = 10;
    [ObservableProperty]
    private bool continuousMeasurement = false;

    public MainWindowViewModel(IOtdrDevice device, ConnectionManager connectionManager)
    {
        _device = device;
        _connectionManager = connectionManager;

        CurrentTrace = _device.LatestTrace;
        IsConnected = _device.IsConnected;
        IsLiveAcquiring = _device.IsAcquiring;

        _device.TraceReceived += OnTraceReceived;
        _device.AcquisitionFaulted += OnAcquisitionFaulted;
        _averager = new TraceAverage(1);
    }

    // Design-time only — do not use for runtime construction
    public MainWindowViewModel() : this(new DesignTimeOtdrDeviceService(), new ConnectionManager(Array.Empty<IConnectionProvider>()))
    {
    }

    // Commands
    private bool CanConnect() => SelectedEndpoint is not null && !IsConnected;
    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync()
    {
        if (SelectedEndpoint is null) return;
        if(_device.IsConnected) return;

        ConnectionStatus = ConnectionStatus_e.Connecting;
        try
        {
            await _device.ConnectAsync(SelectedEndpoint);
            IsConnected = _device.IsConnected;
            ConnectionStatus = ConnectionStatus_e.Connected;
        }
        catch (Exception ex)
        {
            // TODO: Handle exception
            IsConnected = false;
        }

        ConnectCommand.NotifyCanExecuteChanged();
        DisconnectCommand.NotifyCanExecuteChanged();
        AcquireCommand.NotifyCanExecuteChanged();
        StartLiveAcquisitionCommand.NotifyCanExecuteChanged();
        CancelLiveAcquisitionCommand.NotifyCanExecuteChanged();
    }
    private bool CanDisconnect() => IsConnected;

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    public void Disconnect()
    {
        _liveAcquisitionCts?.Cancel();
        _device.StopLiveAcquisition();
        _device.Disconnect();

        IsConnected = false;
        IsLiveAcquiring = false;
        ConnectionStatus = ConnectionStatus_e.Disconnected;

        ConnectCommand.NotifyCanExecuteChanged();
        DisconnectCommand.NotifyCanExecuteChanged();
        AcquireCommand.NotifyCanExecuteChanged();
        StartLiveAcquisitionCommand.NotifyCanExecuteChanged();
        CancelLiveAcquisitionCommand.NotifyCanExecuteChanged();
    }

    // Acquisition - TODO: this should have its own class
    // TODO: Make averager update averaging window in live mode

    private int _acquisitionCounter = 0;
    private ITraceAverage _averager;
    public ITraceAverage Averager => _averager;
    [RelayCommand(CanExecute = nameof(CanAcquire))]
    private async Task AcquireAsync()
    {
        var settings = new AcquisitionSettings { OnTimeNs = OnTime };
        CurrentTrace = await _device.AcquireTraceAsync(settings);
    }

    private bool CanAcquire() => IsConnected && !IsLiveAcquiring;

    [RelayCommand(CanExecute = nameof(CanAcquire))]
    private async Task StartLiveAcquisition()
    {
        IsLiveAcquiring = true;
        _liveAcquisitionCts = new CancellationTokenSource();
        _averager = new TraceAverage(SoftwareAveraging ?? 1);
        var settings = new AcquisitionSettings { OnTimeNs = OnTime };
        await _device.StartLiveAcquisitionAsync(settings, _liveAcquisitionCts.Token);
        AcquireCommand.NotifyCanExecuteChanged();
    }

    private void OnTraceReceived(object? sender, TraceData trace)
    {
        var now = DateTimeOffset.UtcNow;
        if (now - _lastUiUpdate < MinUiInterval) return;
        _lastUiUpdate = now;
        _acquisitionCounter++;
        bool stop = !_averager.Add(trace);
        if(_acquisitionCounter > SoftwareAveraging && !ContinuousMeasurement) stop = true;
        if(stop) CancelLiveAcquisition();
        if(!stop) Dispatcher.UIThread.Post(() => CurrentTrace = _averager.GetResult());
    }

    private void OnAcquisitionFaulted(object? sender, Exception ex)
    {
        Dispatcher.UIThread.Post(() =>
        {
            // IsLiveAcquiring = false;
            CancelLiveAcquisition();
            // TODO: Handle faults in infotext
            // ConnectionStatus = $"Live acquisition error: {ex.Message}";
        });
    }
    
    private bool CanStopAcquring() => IsConnected && IsLiveAcquiring;
    [RelayCommand(CanExecute = nameof(CanStopAcquring))]
    private void CancelLiveAcquisition()
    {
        _liveAcquisitionCts?.Cancel();
        _device.StopLiveAcquisition();
        IsLiveAcquiring = false;
        _averager.Reset();
        _acquisitionCounter = 0;
    }

    // Input formatting stuff
    // On Time (ns <-> "1us"-style text)
    private double _onTimeNs = 1000.0;
    private string _onTimeStr = "1us";

    public double OnTime
    {
        get => _onTimeNs;
        set
        {
            if (Math.Abs(_onTimeNs - value) < 0.0001) return;
            _onTimeNs = value;
            OnPropertyChanged(nameof(OnTime));

            var formatted = FormatNs(value);
            if (_onTimeStr != formatted)
            {
                _onTimeStr = formatted;
                OnPropertyChanged(nameof(OnTimeStr));
            }
        }
    }

    public string OnTimeStr
    {
        get => _onTimeStr;
        set
        {
            if (_onTimeStr == value) return;

            _onTimeStr = value;
            OnPropertyChanged(nameof(OnTimeStr));

            if (TryParseToNs(value, out double ns))
                OnTime = ns;
        }
    }

    private static string FormatNs(double ns) => ns switch
    {
        < 1_000 => $"{ns:0}ns",
        < 1_000_000 => $"{ns / 1_000:0.##}us",
        _ => $"{ns / 1_000_000:0.##}ms"
    };

    private static bool TryParseToNs(string text, out double ns)
    {
        ns = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;

        text = text.Trim().ToLowerInvariant();
        double multiplier;
        string numberPart;

        if (text.EndsWith("ns")) { multiplier = 1.0; numberPart = text[..^2]; }
        else if (text.EndsWith("n")) { multiplier = 1.0; numberPart = text[..^1]; }
        else if (text.EndsWith("us") || text.EndsWith("µs")) { multiplier = 1_000.0; numberPart = text[..^2]; }
        else if (text.EndsWith("u") || text.EndsWith("µ")) {{ multiplier = 1_000.0; numberPart = text[..^1]; }}
        else if (text.EndsWith("ms") || text.EndsWith("m")) { multiplier = 1_000_000.0; numberPart = text[..^2]; }
        else if (text.EndsWith("m")) { multiplier = 1_000_000.0; numberPart = text[..^1]; }
        else return false;

        if (!double.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture, out double num))
            return false;

        ns = num * multiplier;
        return true;
    }
}

// Designer
internal class DesignTimeOtdrDeviceService : IOtdrDevice
{
    public bool IsConnected => false;
    public bool IsAcquiring => false;
    public TraceData? LatestTrace => null;

    public event EventHandler<TraceData>? TraceReceived { add {} remove {} }
    public event EventHandler<Exception>? AcquisitionFaulted { add {} remove {} }

    public Task ConnectAsync(DeviceEndpoint endpoint) => Task.CompletedTask;

    public void Disconnect() { }

    public Task<TraceData> AcquireTraceAsync(AcquisitionSettings settings)
        => Task.FromResult(new TraceData
        {
            DistanceKm = Array.Empty<double>(),
            SignalDbm = Array.Empty<double>()
        });

    public Task StartLiveAcquisitionAsync(AcquisitionSettings settings, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public void StopLiveAcquisition() { }
}