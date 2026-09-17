using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IThemeService _themeService;
    private readonly INavigationService _navigationService;
    private readonly ISystemInfoService _systemInfoService;
    private readonly IPrinterRoleDetectionService _roleDetectionService;
    private readonly INetworkService _networkService;

    [ObservableProperty]
    private string _appName = "MQ Printer Fixer";

    [ObservableProperty]
    private string _subtitle = "Diagnose and repair LAN printer problems";

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _currentThemeDisplay = string.Empty;

    [ObservableProperty]
    private PageViewModelBase? _currentPage;

    [ObservableProperty]
    private NavigationItem? _selectedNavigationItem;

    [ObservableProperty]
    private string _computerName = "...";

    [ObservableProperty]
    private string _windowsEditionDisplay = "...";

    [ObservableProperty]
    private bool _isAdministrator;

    public string AdministratorDisplay => IsAdministrator ? "Yes" : "No";

    [ObservableProperty]
    private string _printerRoleDisplay = "...";

    [ObservableProperty]
    private string _networkProfileDisplay = "...";

    public ObservableCollection<NavigationItem> NavigationItems { get; } = new()
    {
        new(NavigationItemKey.Dashboard,      "Dashboard",         "\uE80F"),
        new(NavigationItemKey.Printers,       "Printers",          "\uE749"),
        new(NavigationItemKey.NetworkSharing, "Network & Sharing", "\uE968"),
        new(NavigationItemKey.Error0x11B,     "0x0000011B",        "\uE7BA"),
        new(NavigationItemKey.Error0x709,     "0x00000709",        "\uE7BA"),
        new(NavigationItemKey.Advanced,       "Advanced",          "\uE713"),
        new(NavigationItemKey.Logs,           "Logs",              "\uE9D9"),
        new(NavigationItemKey.Settings,       "Settings",          "\uE713")
    };

    public MainViewModel(
        IThemeService themeService,
        INavigationService navigationService,
        ISystemInfoService systemInfoService,
        IPrinterRoleDetectionService roleDetectionService,
        INetworkService networkService)
    {
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _systemInfoService = systemInfoService ?? throw new ArgumentNullException(nameof(systemInfoService));
        _roleDetectionService = roleDetectionService ?? throw new ArgumentNullException(nameof(roleDetectionService));
        _networkService = networkService ?? throw new ArgumentNullException(nameof(networkService));

        Title = AppName;

        _themeService.ThemeChanged += (_, _) => UpdateThemeDisplay();
        _navigationService.CurrentPageChanged += (_, _) => CurrentPage = _navigationService.CurrentPage;

        UpdateThemeDisplay();

        _navigationService.NavigateToDefault();
        CurrentPage = _navigationService.CurrentPage;
        SelectedNavigationItem = NavigationItems[0];

        _ = LoadSystemInfoAsync();
        _ = LoadPrinterRoleAsync();
        _ = LoadNetworkAsync();
    }

    [RelayCommand]
    private void CycleTheme()
    {
        var next = _themeService.CurrentTheme switch
        {
            AppTheme.System => AppTheme.Light,
            AppTheme.Light  => AppTheme.Dark,
            AppTheme.Dark   => AppTheme.System,
            _ => AppTheme.System
        };
        _themeService.ApplyTheme(next);
    }

    [RelayCommand]
    public async Task LoadSystemInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var info = await _systemInfoService.GetComputerInfoAsync(cancellationToken);
            ComputerName = info.ComputerName;
            WindowsEditionDisplay = info.EditionDisplayName;
            IsAdministrator = info.IsAdministrator;
        }
        catch { }
    }

    [RelayCommand]
    public async Task LoadPrinterRoleAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _roleDetectionService.DetectAsync(RoleDetectionMode.Auto, cancellationToken);
            PrinterRoleDisplay = PrinterRoleMapper.ToDisplayName(result.EffectiveRole);
        }
        catch
        {
            PrinterRoleDisplay = "Unknown";
        }
    }

    [RelayCommand]
    public async Task LoadNetworkAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var network = await _networkService.GetActiveNetworkAsync(cancellationToken);
            NetworkProfileDisplay = network.IsConnected
                ? NetworkCategoryMapper.ToDisplayName(network.Category)
                : "Disconnected";
        }
        catch
        {
            NetworkProfileDisplay = "Unknown";
        }
    }

    partial void OnSelectedNavigationItemChanged(NavigationItem? value)
    {
        if (value is null) return;
        _navigationService.Navigate(value.Key);
    }

    partial void OnIsAdministratorChanged(bool value) => OnPropertyChanged(nameof(AdministratorDisplay));

    private void UpdateThemeDisplay()
    {
        CurrentThemeDisplay = _themeService.CurrentTheme switch
        {
            AppTheme.System => $"System ({_themeService.EffectiveTheme})",
            _ => _themeService.CurrentTheme.ToString()
        };
    }
}