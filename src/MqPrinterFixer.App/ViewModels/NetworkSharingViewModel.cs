using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.ViewModels;

public sealed partial class NetworkSharingViewModel : PageViewModelBase
{
    private readonly INetworkService _networkService;
    private readonly INetworkSharingService _sharingService;

    public override NavigationItemKey Key => NavigationItemKey.NetworkSharing;

    [ObservableProperty]
    private NetworkInfo _network = NetworkInfo.Empty;

    [ObservableProperty]
    private SharingState _sharingState = SharingState.Unknown;

    [ObservableProperty]
    private NetworkChangePreview? _activePreview;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isApplying;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _applyResultMessage = string.Empty;

    [ObservableProperty]
    private bool _applyResultIsSuccess;

    // ----- Display properties -----

    public string CategoryDisplay =>
        NetworkCategoryMapper.ToDisplayName(Network.Category);

    public string ConnectionDisplay =>
        Network.IsConnected ? "Connected" : "Disconnected";

    public string NetworkDiscoveryDisplay =>
        SharingState.NetworkDiscoveryEnabled ? "Enabled" : "Disabled";

    public string FileAndPrinterSharingDisplay =>
        SharingState.FileAndPrinterSharingEnabled ? "Enabled" : "Disabled";

    public string PasswordProtectedSharingDisplay =>
        SharingState.PasswordProtectedSharingEnabled ? "ON" : "OFF";

    public NetworkSharingViewModel(
        INetworkService networkService,
        INetworkSharingService sharingService)
    {
        _networkService = networkService ?? throw new ArgumentNullException(nameof(networkService));
        _sharingService = sharingService ?? throw new ArgumentNullException(nameof(sharingService));

        Title = "Network & Sharing";
        Description = "Thông tin mạng LAN và trạng thái chia sẻ.";

        _ = LoadAsync();
    }

    partial void OnNetworkChanged(NetworkInfo value)
    {
        OnPropertyChanged(nameof(CategoryDisplay));
        OnPropertyChanged(nameof(ConnectionDisplay));
    }

    partial void OnSharingStateChanged(SharingState value)
    {
        OnPropertyChanged(nameof(NetworkDiscoveryDisplay));
        OnPropertyChanged(nameof(FileAndPrinterSharingDisplay));
        OnPropertyChanged(nameof(PasswordProtectedSharingDisplay));
    }

    [RelayCommand]
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        StatusMessage = "Scanning network...";
        ActivePreview = null;
        ApplyResultMessage = string.Empty;

        try
        {
            var networkTask = _networkService.GetActiveNetworkAsync(cancellationToken);
            var sharingTask = _sharingService.GetSharingStateAsync(cancellationToken);

            await Task.WhenAll(networkTask, sharingTask);

            Network = await networkTask;
            SharingState = await sharingTask;
            StatusMessage = Network.IsConnected
                ? $"{Network.AdapterName} · {CategoryDisplay}"
                : "Không có kết nối mạng.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    // -----------------------------------------------------------------
    //   Preview + Apply + Cancel
    // -----------------------------------------------------------------

    [RelayCommand]
    private async Task PreviewChangeAsync(NetworkChangeAction action)
    {
        ApplyResultMessage = string.Empty;
        ActivePreview = await _sharingService.PreviewChangeAsync(action);
    }

    [RelayCommand]
    private void CancelPreview()
    {
        ActivePreview = null;
        ApplyResultMessage = string.Empty;
    }

    [RelayCommand]
    private async Task ApplyChangeAsync()
    {
        if (ActivePreview is null || IsApplying)
        {
            return;
        }

        IsApplying = true;
        try
        {
            var result = await _sharingService.ApplyChangeAsync(ActivePreview);

            ApplyResultIsSuccess = result.Success && result.Verified;
            ApplyResultMessage = result.Success
                ? (result.Verified
                    ? $"Thành công: {result.BeforeState} → {result.AfterState}"
                    : result.ErrorMessage)
                : $"Thất bại: {result.ErrorMessage}";

            // Reload state sau apply
            await LoadAsync();
        }
        finally
        {
            IsApplying = false;
        }
    }
}