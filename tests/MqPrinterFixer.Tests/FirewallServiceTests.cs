using MqPrinterFixer.App.Models;
using MqPrinterFixer.App.Services;
using MqPrinterFixer.Tests.Fakes;
using Xunit;

namespace MqPrinterFixer.Tests;

public class FirewallServiceTests
{
    [Fact]
    public async Task CheckAsync_PowerShellFails_ReturnsUnknown()
    {
        var fake = new FakePowerShellService
        {
            NextResult = PowerShellResult.Fail(1, "error")
        };
        var svc = new FirewallService(fake, new SystemInfoService());

        var status = await svc.CheckAsync();

        Assert.Same(FirewallStatus.Unknown, status);
    }

    [Fact]
    public async Task CheckAsync_ValidJson_ReturnsStatus()
    {
        var json = """
        [
            {"GroupName":"File and Printer Sharing","TotalRules":10,"EnabledRules":10},
            {"GroupName":"Network Discovery","TotalRules":8,"EnabledRules":8}
        ]
        """;

        var fake = new FakePowerShellService { NextResult = PowerShellResult.Ok(json) };
        var svc = new FirewallService(fake, new SystemInfoService());

        var status = await svc.CheckAsync();

        Assert.Equal(FirewallRuleStatus.Enabled, status.OverallStatus);
        Assert.Equal(2, status.Checks.Count);
    }
}