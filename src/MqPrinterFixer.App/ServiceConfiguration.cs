using Microsoft.Extensions.DependencyInjection;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Services;
using MqPrinterFixer.App.ViewModels;

namespace MqPrinterFixer.App;

/// <summary>
/// Đăng ký toàn bộ service, ViewModel, Window vào DI container.
/// Tách riêng khỏi <see cref="App"/> để test project có thể dùng lại.
/// </summary>
public static class ServiceConfiguration
{
    public static IServiceCollection AddMqPrinterFixerServices(this IServiceCollection services)
    {
        // ----- Services -----
        services.AddSingleton<IThemeService, ThemeService>();

        // ----- ViewModels -----
        services.AddSingleton<MainViewModel>();

        // ----- Windows -----
        services.AddSingleton<MainWindow>();

        // Các service nghiệp vụ khác sẽ đăng ký ở Phase 8+
        return services;
    }
}