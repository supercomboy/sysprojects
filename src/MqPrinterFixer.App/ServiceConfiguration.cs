using Microsoft.Extensions.DependencyInjection;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Services;
using MqPrinterFixer.App.ViewModels;

namespace MqPrinterFixer.App;

public static class ServiceConfiguration
{
    public static IServiceCollection AddMqPrinterFixerServices(this IServiceCollection services)
    {
        // ----- Infrastructure Services -----
        services.AddSingleton<IPowerShellService, PowerShellService>();
        services.AddSingleton<ISystemInfoService, SystemInfoService>();
        services.AddSingleton<IComputerService, ComputerService>();
        services.AddSingleton<IPrinterService, PrinterService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<INavigationService, NavigationService>();

        // ----- Page ViewModels -----
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<PrintersViewModel>();
        services.AddSingleton<NetworkSharingViewModel>();
        services.AddSingleton<Error0x11BViewModel>();
        services.AddSingleton<Error0x709ViewModel>();
        services.AddSingleton<AdvancedViewModel>();
        services.AddSingleton<LogsViewModel>();
        services.AddSingleton<SettingsViewModel>();

        // ----- Dialog ViewModels (transient) -----
        services.AddTransient<RenameComputerDialogViewModel>();

        // ----- Shell ViewModel & Window -----
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        return services;
    }
}