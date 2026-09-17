using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.ViewModels;

public sealed partial class DashboardViewModel : PageViewModelBase
{
    private readonly ISystemInfoService _systemInfoService;
    private readonly IPrinterRoleDetectionService _roleDetectionService;
    private readonly INetworkService _networkService;

    public override NavigationItemKey Key => NavigationItemKey.Dashboard;

    [ObservableProperty]
    private ComputerInfo? _computerInfo;

    [ObservableProperty]
    private PrinterRoleDetectionResult? _roleResult;

    [ObservableProperty]
    private NetworkInfo _network = NetworkInfo.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public string PrinterRoleDisplay =>
        RoleResult is null ? "..." : PrinterRoleMapper.ToDisplayName(RoleResult.EffectiveRole);

    public string PrinterRoleDetail =>
        RoleResult is null
            ? string.Empty
            : $"Local shared: {RoleResult.LocalSharedPrinterCount} · Remote: {RoleResult.RemoteSharedPrinterCount}";

    public string NetworkCategoryDisplay =>
        Network.IsConnected ? NetworkCategoryMapper.ToDisplayName(Network.Category) : "Disconnected";

    public string NetworkDetail =>
        Network.IsConnected
            ? $"{Network.AdapterName} · {Network.IPv4Address}"
            : "Không có kết nối mạng.";

    public DashboardViewModel(
        ISystemInfoService systemInfoService,
        IPrinterRoleDetectionService roleDetectionService,
        INetworkService networkService)
    {
        _systemInfoService = systemInfoService
            ?? throw new ArgumentNullException(nameof(systemInfoService));
        _roleDetectionService = roleDetectionService
            ?? throw new ArgumentNullException(nameof(roleDetectionService));
        _networkService = networkService
            ?? throw new ArgumentNullException(nameof(networkService));

        Title = "Dashboard";
        Description = "System overview — thông tin đọc từ Windows.";

        _ = LoadAsync();
    }

    partial void OnRoleResultChanged(PrinterRoleDetectionResult? value)
    {
        OnPropertyChanged(nameof(PrinterRoleDisplay));
        OnPropertyChanged(nameof(PrinterRoleDetail));
    }

    partial void OnNetworkChanged(NetworkInfo value)
    {
        OnPropertyChanged(nameof(NetworkCategoryDisplay));
        OnPropertyChanged(nameof(NetworkDetail));
    }

    [RelayCommand]
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        try
        {
            var infoTask = _systemInfoService.GetComputerInfoAsync(cancellationToken);
            var roleTask = _roleDetectionService.DetectAsync(RoleDetectionMode.Auto, cancellationToken);
            var netTask  = _networkService.GetActiveNetworkAsync(cancellationToken);

            await Task.WhenAll(infoTask, roleTask, netTask);

            ComputerInfo = await infoTask;
            RoleResult = await roleTask;
            Network = await netTask;
        }
        catch { }
        finally
        {
            IsLoading = false;
        }
    }
}