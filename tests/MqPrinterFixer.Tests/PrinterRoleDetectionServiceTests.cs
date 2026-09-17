using MqPrinterFixer.App.Models;
using MqPrinterFixer.App.Services;
using MqPrinterFixer.Tests.Fakes;
using Xunit;

namespace MqPrinterFixer.Tests;

public class PrinterRoleDetectionServiceTests
{
    [Fact]
    public async Task DetectAsync_NoPrinters_ReturnsStandaloneAuto()
    {
        var fake = new FakePrinterService();
        var svc = new PrinterRoleDetectionService(fake);

        var result = await svc.DetectAsync();

        Assert.Equal(PrinterRole.Standalone, result.DetectedRole);
        Assert.Equal(PrinterRole.Standalone, result.EffectiveRole);
        Assert.Equal(RoleDetectionMode.Auto, result.Mode);
        Assert.Equal(0, result.TotalPrinterCount);
        Assert.Equal(0, result.LocalSharedPrinterCount);
        Assert.Equal(0, result.RemoteSharedPrinterCount);
    }

    [Fact]
    public async Task DetectAsync_LocalShared_ReturnsHost()
    {
        var fake = new FakePrinterService();
        fake.PrintersToReturn.Add(FakePrinterService.Build(
            "Canon LBP2900",
            shared: true,
            shareName: "Canon2900"));

        var svc = new PrinterRoleDetectionService(fake);
        var result = await svc.DetectAsync();

        Assert.Equal(PrinterRole.Host, result.DetectedRole);
        Assert.Equal(1, result.LocalSharedPrinterCount);
        Assert.Equal(0, result.RemoteSharedPrinterCount);
    }

    [Fact]
    public async Task DetectAsync_ForceHostOverride_ChangesEffectiveRole()
    {
        // Detected = Standalone nhưng user ép Host
        var fake = new FakePrinterService();
        var svc = new PrinterRoleDetectionService(fake);

        var result = await svc.DetectAsync(RoleDetectionMode.ForceHost);

        Assert.Equal(PrinterRole.Standalone, result.DetectedRole);
        Assert.Equal(PrinterRole.Host, result.EffectiveRole);
        Assert.Equal(RoleDetectionMode.ForceHost, result.Mode);
    }

    [Fact]
    public async Task DetectAsync_ForceClientOverride_ChangesEffectiveRole()
    {
        var fake = new FakePrinterService();
        fake.PrintersToReturn.Add(FakePrinterService.Build(
            "Canon", shared: true, shareName: "Canon2900"));

        var svc = new PrinterRoleDetectionService(fake);
        var result = await svc.DetectAsync(RoleDetectionMode.ForceClient);

        Assert.Equal(PrinterRole.Host, result.DetectedRole);
        Assert.Equal(PrinterRole.Client, result.EffectiveRole);
    }

    [Fact]
    public async Task DetectAsync_ForceHybrid_OverridesEverything()
    {
        var fake = new FakePrinterService();
        var svc = new PrinterRoleDetectionService(fake);

        var result = await svc.DetectAsync(RoleDetectionMode.ForceHybrid);

        Assert.Equal(PrinterRole.Standalone, result.DetectedRole);
        Assert.Equal(PrinterRole.Hybrid, result.EffectiveRole);
    }
}