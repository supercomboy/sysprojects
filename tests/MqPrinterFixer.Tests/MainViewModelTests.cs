using Microsoft.Extensions.DependencyInjection;
using MqPrinterFixer.App;
using MqPrinterFixer.App.Models;
using MqPrinterFixer.App.Services;
using MqPrinterFixer.App.ViewModels;
using Xunit;

namespace MqPrinterFixer.Tests;

public class MainViewModelTests
{
    [Fact]
    public void MainViewModel_HasExpectedDefaults()
    {
        var vm = new MainViewModel(new ThemeService());

        Assert.Equal("MQ Printer Fixer", vm.AppName);
        Assert.Equal("Diagnose and repair LAN printer problems", vm.Subtitle);
        Assert.Equal("Phase 5 — Theme system ready", vm.StatusMessage);
        Assert.Equal("MQ Printer Fixer", vm.Title);
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public void MainViewModel_CanBeResolvedFromDi()
    {
        var services = new ServiceCollection();
        services.AddMqPrinterFixerServices();

        using var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<MainViewModel>();

        Assert.NotNull(vm);
        Assert.IsType<MainViewModel>(vm);
    }

    [Fact]
    public void MainViewModel_PropertyChange_RaisesNotification()
    {
        var vm = new MainViewModel(new ThemeService());
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.StatusMessage = "Scanning system...";

        Assert.Contains(nameof(MainViewModel.StatusMessage), raised);
        Assert.Equal("Scanning system...", vm.StatusMessage);
    }

    [Fact]
    public void MainViewModel_CycleTheme_ChangesCurrentTheme()
    {
        var themeService = new ThemeService();
        var vm = new MainViewModel(themeService);

        // Mặc định System
        Assert.Equal(AppTheme.System, themeService.CurrentTheme);

        // Gọi command 3 lần → System → Light → Dark → System
        vm.CycleThemeCommand.Execute(null);
        Assert.Equal(AppTheme.Light, themeService.CurrentTheme);

        vm.CycleThemeCommand.Execute(null);
        Assert.Equal(AppTheme.Dark, themeService.CurrentTheme);

        vm.CycleThemeCommand.Execute(null);
        Assert.Equal(AppTheme.System, themeService.CurrentTheme);
    }
}