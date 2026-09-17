using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;
using MqPrinterFixer.App.Services;
using Xunit;

namespace MqPrinterFixer.Tests;

public class SpoolerServiceTests
{
    [Fact]
    public async Task GetStatusAsync_DoesNotThrow()
    {
        var svc = new SpoolerService(new SystemInfoService());
        var info = await svc.GetStatusAsync();

        Assert.NotNull(info);
        // Spooler luôn tồn tại trên Windows consumer.
        // Nếu không có quyền đọc, Status sẽ là Unknown — vẫn OK.
    }

    [Fact]
    public async Task StartAsync_WhenNotAdmin_ReturnsBlocked()
    {
        // Trong môi trường test không elevated → phải trả blocked.
        var sysInfo = new SystemInfoService();
        var info = await sysInfo.GetComputerInfoAsync();

        if (info.IsAdministrator)
        {
            // Môi trường test đang elevated — bỏ qua test này.
            return;
        }

        var svc = new SpoolerService(sysInfo);
        var result = await svc.StartAsync();

        Assert.False(result.Success);
        Assert.Contains("Administrator", result.ErrorMessage);
    }

    [Fact]
    public async Task RestartAsync_WhenNotAdmin_ReturnsBlocked()
    {
        var sysInfo = new SystemInfoService();
        var info = await sysInfo.GetComputerInfoAsync();

        if (info.IsAdministrator)
        {
            return;
        }

        var svc = new SpoolerService(sysInfo);
        var result = await svc.RestartAsync();

        Assert.False(result.Success);
        Assert.Contains("Administrator", result.ErrorMessage);
    }
}