using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.ViewModels;

public sealed partial class DashboardViewModel : PageViewModelBase
{
    private readonly ISystemInfoService _systemInfoService;

    public override NavigationItemKey Key => NavigationItemKey.Dashboard;

    [ObservableProperty]
    private ComputerInfo? _computerInfo;

    [ObservableProperty]
    private bool _isLoading;

    public DashboardViewModel(ISystemInfoService systemInfoService)
    {
        _systemInfoService = systemInfoService
            ?? throw new ArgumentNullException(nameof(systemInfoService));

        Title = "Dashboard";
        Description = "System overview — thông tin đọc từ Windows.";

        // Load lần đầu (fire-and-forget, exception đã được nuốt bên trong service).
        _ = LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        try
        {
            ComputerInfo = await _systemInfoService.GetComputerInfoAsync(cancellationToken);
        }
        catch
        {
            // Không crash UI nếu service lỗi — giữ ComputerInfo = null.
        }
        finally
        {
            IsLoading = false;
        }
    }
}