using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Styling;
using OTDR.Views.Preferences;
using OTDR.Views.About;
using OTDR.Views.ViewModels;
using OTDR.Core.Interfaces;
using OTDR.Core.Services.Connections;
using OTDR.Core.Models.Connections;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using OTDR.Core.Models.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace OTDR.Views;

public partial class MainWindow : Window
{
    private readonly IPlotView _plot = null!;
    private readonly Random _random = new();
    private readonly MainWindowViewModel _vm = new();

    private GridLength _lastSettingsWidth = new(280);
    private bool _settingsVisible = true;
    private readonly ISettingsService _settings = null!;
    private readonly IFileDialogService _fileDialogs = null!;
    private readonly ConnectionManager _connectionManager = new();
    private readonly ITransportFactory _transportFactory = new TransportFactory();
    private IScpiTransport? _transport;
    private readonly ILogger<MainWindow> _logger = null!;
    private readonly IServiceProvider _services = null!;

    public MainWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new MainWindowViewModel();
        }
    }

    public MainWindow(ISettingsService settings, IPlotView plot, IFileDialogService fileDialogs, ILogger<MainWindow> logger, IServiceProvider services)
    {
        InitializeComponent();
        _settings = settings;
        _plot = plot;
        _fileDialogs = fileDialogs;
        _logger = logger;
        _services = services;
        PlotContainer.Content = _plot.AsControl();
        Closing += OnWindowClosing;

        DataContext = _vm;
        _vm.PropertyChanged += OnViewModelPropertyChanged;

        SetupInitialPlot();
    }
    public void ApplySettings()
    {
        Width = _settings.Settings.WindowWidth;
        Height = _settings.Settings.WindowHeight;
        Application.Current!.RequestedThemeVariant =
            _settings.Settings.CurrentTheme switch
            {
                AppSettings.Theme.Dark => ThemeVariant.Dark,
                AppSettings.Theme.Light => ThemeVariant.Light,
                _ => ThemeVariant.Default,
            };
    }

    private void SetupInitialPlot()
    {
        _plot.SetTitle("OTDR");
        _plot.SetAxisLabels("Distance [km]", "Signal [dBm]");
        _plot.ShowGrid(_vm.ShowGrid);
        PlotGeneratedData();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MainWindowViewModel.OnTime):
            case nameof(MainWindowViewModel.LineWidth):
            case nameof(MainWindowViewModel.ShowMarkers):
            case nameof(MainWindowViewModel.ShowLegend):
                PlotGeneratedData();
                break;

            case nameof(MainWindowViewModel.ShowGrid):
                _plot.ShowGrid(_vm.ShowGrid);
                break;
        }
    }

    private void PlotGeneratedData()
    {
        double frequency = _vm.OnTime / 1000;
        int count = 1000;
        double[] xs = new double[count];
        double[] ys = new double[count];

        for (int i = 0; i < count; i++)
        {
            double x = i / (double)count * 10.0;
            double y = Math.Sin(2 * Math.PI * frequency * x / 10.0);
            xs[i] = x;
            ys[i] = y;
        }

        _plot.PlotScatter(xs, ys);
        _plot.LineWidth = (float)_vm.LineWidth;
        _plot.MarkerSize = _vm.ShowMarkers ? 5 : 0;
        _plot.LegendText = _vm.ShowLegend ? "Example series" : null;
        _plot.AutoScale();
    }

    private void OnPlotClick(object? sender, RoutedEventArgs e)
    {
        InfoText.Text = "Plotted";
        PlotGeneratedData();
    }

    private void OnConnectionClick(object? sender, RoutedEventArgs e)
    {
        RefreshConnectionMenu();
    }

    private void RefreshConnectionMenu()
    {
        while (ConnectionMenuItem.Items.Count > 2)
            ConnectionMenuItem.Items.RemoveAt(2);

        foreach (var provider in _connectionManager.Providers)
        {
            var submenu = new MenuItem { Header = provider.Name };

            foreach (var endpoint in provider.GetConnections())
            {
                var item = new MenuItem
                {
                    Header = endpoint.DisplayName,
                    Tag = endpoint
                };
                item.Click += OnEndpointSelected;
                submenu.Items.Add(item);
            }

            if (submenu.Items.Count == 0)
                submenu.Items.Add(new MenuItem { Header = "(none found)", IsEnabled = false });

            ConnectionMenuItem.Items.Add(submenu);
        }
    }
    private async void OnEndpointSelected(object? sender, RoutedEventArgs e)
    {
        if(sender is not MenuItem { Tag: DeviceEndpoint endpoint } item) return;

        _transport?.Dispose();
        _transport = _transportFactory.Create(endpoint);

        try
        {
            await Task.Run(() => _transport.Connect());
            _logger.LogInformation("Connected to {Endpoint}", endpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to {Endpoint}", endpoint);
            _transport = null;
        }
    }

    private void OnStartClick(object? sender, RoutedEventArgs e)
    {
        _vm.OnTime = _random.NextDouble() * 5;
        InfoText.Text = "Randomized";
    }

    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        _plot.Clear();
        InfoText.Text = "Cleared";
    }

    private async void OnExportClick(object? sender, RoutedEventArgs e)
    {
        var path = await _fileDialogs.ShowSaveCsvDialogAsync(this);

        if (path is null)
        {
            InfoText.Text = "Export cancelled";
            return;
        }

        InfoText.Text = $"Selected: {path}";
        // _plot.SavePng("chart_export.png", 1000, 600);
        // InfoText.Text = "Exported to chart_export.png";
    }

    private void OnExitClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }

    private async void OnPreferencesClick(object? sender, RoutedEventArgs e)
    {
        var prefWindow = _services.GetRequiredService<PreferencesWindow>();
        await prefWindow.ShowDialog(this);
    }

    private void OnToggleSettingsClick(object? sender, RoutedEventArgs e)
    {
        _settingsVisible = !_settingsVisible;
        ApplySettingsVisibility();
    }

    private void ApplySettingsVisibility()
    {
        var columnDefs = RootGrid.ColumnDefinitions;

        if (_settingsVisible)
        {
            columnDefs[0].Width = _lastSettingsWidth;
            columnDefs[1].Width = new GridLength(4);
            SettingsPanel.IsVisible = true;
            SettingsSplitter.IsVisible = true;
            SettingsToggleButton.Content = "◀";
        }
        else
        {
            if (columnDefs[0].Width.Value > 0)
                _lastSettingsWidth = columnDefs[0].Width;

            columnDefs[0].Width = new GridLength(0);
            columnDefs[1].Width = new GridLength(0);
            SettingsPanel.IsVisible = false;
            SettingsSplitter.IsVisible = false;
            SettingsToggleButton.Content = "▶";
        }

        SettingsToggleButton.IsChecked = _settingsVisible;
    }

    private void OnDocsClick(object? sender, RoutedEventArgs e) => InfoText.Text = "Documentation (stub)";

    private async void OnAboutClick(object? sender, RoutedEventArgs e)
    {
        var aboutWindow = new AboutWindow();
        await aboutWindow.ShowDialog(this);
    }
    private async void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        _settings.Settings.WindowWidth = Width;
        _settings.Settings.WindowHeight = Height;
    }
}