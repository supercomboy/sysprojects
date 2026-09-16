using Microsoft.Extensions.DependencyInjection;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;
using MqPrinterFixer.App.ViewModels;

namespace MqPrinterFixer.App.Services;

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;
    private PageViewModelBase? _currentPage;

    public NavigationService(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public PageViewModelBase? CurrentPage => _currentPage;

    public event EventHandler? CurrentPageChanged;

    public void Navigate(NavigationItemKey key)
    {
        var page = ResolvePage(key);
        if (ReferenceEquals(page, _currentPage))
        {
            return;
        }

        _currentPage = page;
        CurrentPageChanged?.Invoke(this, EventArgs.Empty);
    }

    public void NavigateToDefault() => Navigate(NavigationItemKey.Dashboard);

    private PageViewModelBase ResolvePage(NavigationItemKey key) => key switch
    {
        NavigationItemKey.Dashboard       => _services.GetRequiredService<DashboardViewModel>(),
        NavigationItemKey.Printers        => _services.GetRequiredService<PrintersViewModel>(),
        NavigationItemKey.NetworkSharing  => _services.GetRequiredService<NetworkSharingViewModel>(),
        NavigationItemKey.Error0x11B      => _services.GetRequiredService<Error0x11BViewModel>(),
        NavigationItemKey.Error0x709      => _services.GetRequiredService<Error0x709ViewModel>(),
        NavigationItemKey.Advanced        => _services.GetRequiredService<AdvancedViewModel>(),
        NavigationItemKey.Logs            => _services.GetRequiredService<LogsViewModel>(),
        NavigationItemKey.Settings        => _services.GetRequiredService<SettingsViewModel>(),
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown NavigationItemKey.")
    };
}