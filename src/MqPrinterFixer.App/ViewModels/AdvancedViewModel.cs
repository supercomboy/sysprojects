using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.ViewModels;

public sealed partial class AdvancedViewModel : PageViewModelBase
{
    private readonly IFirewallService _firewallService;
    private readonly ISpoolerService _spoolerService;

    public override NavigationItemKey Key => NavigationItemKey.Advanced;

    [ObservableProperty]
    private FirewallStatus _firewall = FirewallStatus.Unknown;

    [ObservableProperty]
    private SpoolerInfo _spooler = SpoolerInfo.Unknown;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isApplying;

    [ObservableProperty]
    private bool _isApplyingSpooler;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _applyResultMessage = string.Empty;

    [ObservableProperty]
    private bool _applyResultIsSuccess;

    [ObservableProperty]
    private bool _hasApplyResultMessage;

    [ObservableProperty]
    private string _spoolerResultMessage = string.Empty;

    [ObservableProperty]
    private bool _spoolerResultIsSuccess;

    [ObservableProperty]
    private bool _hasSpoolerResultMessage;

    public string FirewallOverallDisplay =>
        FirewallStatusAggregator.ToDisplayName(Firewall.OverallStatus);

    public string SpoolerStatusDisplay =>
        SpoolerStatusMapper.ToDisplayName(Spooler.Status);

    public bool CanStartSpooler => Spooler.CanStart;
    public bool CanRestartSpooler => Spooler.CanRestart;

    public AdvancedViewModel(
        IFirewallService firewallService,
        ISpoolerService spoolerService)
    {
        _firewallService = firewallService ?? throw new ArgumentNullException(nameof(firewallService));
        _spoolerService = spoolerService ?? throw new ArgumentNullException(nameof(spoolerService));

        Title = "Advanced";
        Description = "Firewall, spooler, ports, policies — kỹ thuật viên.";

        _ = LoadAsync();
    }

    partial void OnFirewallChanged(FirewallStatus value)
    {
        OnPropertyChanged(nameof(FirewallOverallDisplay));
    }

    partial void OnSpoolerChanged(SpoolerInfo value)
    {
        OnPropertyChanged(nameof(SpoolerStatusDisplay));
        OnPropertyChanged(nameof(CanStartSpooler));
        OnPropertyChanged(nameof(CanRestartSpooler));
    }

    partial void OnApplyResultMessageChanged(string value)
    {
        HasApplyResultMessage = !string.IsNullOrWhiteSpace(value);
    }

    partial void OnSpoolerResultMessageChanged(string value)
    {
        HasSpoolerResultMessage = !string.IsNullOrWhiteSpace(value);
    }

    [RelayCommand]
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        StatusMessage = "Checking services...";
        ApplyResultMessage = string.Empty;
        SpoolerResultMessage = string.Empty;

        try
        {
            var firewallTask = _firewallService.CheckAsync(cancellationToken);
            var spoolerTask = _spoolerService.GetStatusAsync(cancellationToken);

            await Task.WhenAll(firewallTask, spoolerTask);

            Firewall = await firewallTask;
            Spooler = await spoolerTask;

            StatusMessage = $"{Firewall.Summary} · Spooler: {SpoolerStatusDisplay}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task EnableRequiredFirewallRulesAsync()
    {
        if (IsApplying) return;

        IsApplying = true;
        try
        {
            var result = await _firewallService.EnableRequiredRulesAsync();

            ApplyResultIsSuccess = result.Success && result.Verified;
            ApplyResultMessage = result.Success
                ? (result.Verified
                    ? $"Đã bật firewall rules: {result.BeforeState} → {result.AfterState}"
                    : result.ErrorMessage)
                : $"Thất bại: {result.ErrorMessage}";

            await LoadAsync();
        }
        finally
        {
            IsApplying = false;
        }
    }

    [RelayCommand]
    private async Task StartSpoolerAsync()
    {
        if (IsApplyingSpooler) return;

        IsApplyingSpooler = true;
        try
        {
            var result = await _spoolerService.StartAsync();

            SpoolerResultIsSuccess = result.Success && result.Verified;
            SpoolerResultMessage = result.Success
                ? (result.Verified
                    ? $"Print Spooler: {result.BeforeState} → {result.AfterState}"
                    : result.ErrorMessage)
                : $"Thất bại: {result.ErrorMessage}";

            await LoadAsync();
        }
        finally
        {
            IsApplyingSpooler = false;
        }
    }

    [RelayCommand]
    private async Task RestartSpoolerAsync()
    {
        if (IsApplyingSpooler) return;

        IsApplyingSpooler = true;
        try
        {
            var result = await _spoolerService.RestartAsync();

            SpoolerResultIsSuccess = result.Success && result.Verified;
            SpoolerResultMessage = result.Success
                ? (result.Verified
                    ? $"Print Spooler restarted: {result.BeforeState} → {result.AfterState}"
                    : result.ErrorMessage)
                : $"Thất bại: {result.ErrorMessage}";

            await LoadAsync();
        }
        finally
        {
            IsApplyingSpooler = false;
        }
    }
}