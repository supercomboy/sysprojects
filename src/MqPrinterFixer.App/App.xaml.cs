using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MqPrinterFixer.App.Interfaces;

namespace MqPrinterFixer.App;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public IServiceProvider Services =>
        _serviceProvider
        ?? throw new InvalidOperationException("ServiceProvider chưa được khởi tạo.");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        services.AddMqPrinterFixerServices();
        _serviceProvider = services.BuildServiceProvider();

        // Áp dụng theme trước khi hiển thị bất kỳ Window nào
        // để tránh flash màu sai.
        var themeService = _serviceProvider.GetRequiredService<IThemeService>();
        themeService.Initialize();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        _serviceProvider = null;
        base.OnExit(e);
    }
}