using Microsoft.Extensions.DependencyInjection;
using MqPrinterFixer.App;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;
using MqPrinterFixer.App.ViewModels;
using Xunit;

namespace MqPrinterFixer.Tests;

public class MainViewModelTests
{
    [Fact]
    public void MainViewModel_HasExpectedDefaults()
    {
        using var provider = BuildProvider();
        var vm = provider.GetRequiredService<MainViewModel>();

        Assert.Equal("MQ Printer Fixer", vm.AppName);
        Assert.Equal("Diagnose and repair LAN printer problems", vm.Subtitle);
        Assert.Equal("Ready", vm.StatusMessage);
        Assert.Equal("MQ Printer Fixer", vm.Title);
        Assert.False(vm.IsBusy);
        Assert.Equal(8, vm.NavigationItems.Count);
        Assert.NotNull(vm.CurrentPage);
        Assert.IsType<DashboardViewModel>(vm.CurrentPage);
    }

    [Fact]
    public void MainViewModel_CanBeResolvedFromDi()
    {
        using var provider = BuildProvider();
        var vm = provider.GetRequiredService<MainViewModel>();
        Assert.NotNull(vm);
        Assert.IsType<MainViewModel>(vm);
    }

    [Fact]
    public void MainViewModel_PropertyChange_RaisesNotification()
    {
        using var provider = BuildProvider();
        var vm = provider.GetRequiredService<MainViewModel>();

        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.StatusMessage = "Scanning system...";

        Assert.Contains(nameof(MainViewModel.StatusMessage), raised);
        Assert.Equal("Scanning system...", vm.StatusMessage);
    }

    [Fact]
    public void MainViewModel_CycleTheme_ChangesCurrentTheme()
    {
        using var provider = BuildProvider();
        var vm = provider.GetRequiredService<MainViewModel>();
        var theme = provider.GetRequiredService<IThemeService>();

        Assert.Equal(AppTheme.System, theme.CurrentTheme);

        vm.CycleThemeCommand.Execute(null);
        Assert.Equal(AppTheme.Light, theme.CurrentTheme);

        vm.CycleThemeCommand.Execute(null);
        Assert.Equal(AppTheme.Dark, theme.CurrentTheme);

        vm.CycleThemeCommand.Execute(null);
        Assert.Equal(AppTheme.System, theme.CurrentTheme);
    }

    [Fact]
    public void MainViewModel_SelectingNavigationItem_Navigates()
    {
        using var provider = BuildProvider();
        var vm = provider.GetRequiredService<MainViewModel>();

        var printers = vm.NavigationItems.First(i => i.Key == NavigationItemKey.Printers);
        vm.SelectedNavigationItem = printers;

        Assert.NotNull(vm.CurrentPage);
        Assert.IsType<PrintersViewModel>(vm.CurrentPage);
    }

    [Fact]
    public async Task MainViewModel_LoadSystemInfo_PopulatesProperties()
    {
        using var provider = BuildProvider();
        var vm = provider.GetRequiredService<MainViewModel>();

        await vm.LoadSystemInfoCommand.ExecuteAsync(null);

        Assert.False(string.IsNullOrWhiteSpace(vm.ComputerName));
        Assert.NotEqual("...", vm.ComputerName);
        Assert.False(string.IsNullOrWhiteSpace(vm.WindowsEditionDisplay));
        Assert.NotEqual("...", vm.WindowsEditionDisplay);
        Assert.Equal(vm.IsAdministrator ? "Yes" : "No", vm.AdministratorDisplay);
    }

    [Fact]
    public async Task MainViewModel_LoadPrinterRole_PopulatesDisplay()
    {
        using var provider = BuildProvider();
        var vm = provider.GetRequiredService<MainViewModel>();

        await vm.LoadPrinterRoleCommand.ExecuteAsync(null);

        Assert.False(string.IsNullOrWhiteSpace(vm.PrinterRoleDisplay));
        Assert.NotEqual("...", vm.PrinterRoleDisplay);
    }

    [Fact]
    public async Task MainViewModel_LoadNetwork_PopulatesDisplay()
    {
        using var provider = BuildProvider();
        var vm = provider.GetRequiredService<MainViewModel>();

        await vm.LoadNetworkCommand.ExecuteAsync(null);

        Assert.False(string.IsNullOrWhiteSpace(vm.NetworkProfileDisplay));
        Assert.NotEqual("...", vm.NetworkProfileDisplay);
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddMqPrinterFixerServices();
        return services.BuildServiceProvider();
    }
}