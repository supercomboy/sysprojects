using MqPrinterFixer.App.Models;
using MqPrinterFixer.App.Services;
using Xunit;

namespace MqPrinterFixer.Tests;

public class SystemInfoServiceTests
{
    [Fact]
    public async Task GetComputerInfoAsync_DoesNotThrow()
    {
        var service = new SystemInfoService();
        var info = await service.GetComputerInfoAsync();

        Assert.NotNull(info);
    }

    [Fact]
    public async Task GetComputerInfoAsync_ComputerNameIsNotEmpty()
    {
        var service = new SystemInfoService();
        var info = await service.GetComputerInfoAsync();

        Assert.False(string.IsNullOrWhiteSpace(info.ComputerName));
    }

    [Fact]
    public async Task GetComputerInfoAsync_UserNameIsNotEmpty()
    {
        var service = new SystemInfoService();
        var info = await service.GetComputerInfoAsync();

        Assert.False(string.IsNullOrWhiteSpace(info.UserName));
    }

    [Fact]
    public async Task GetComputerInfoAsync_EditionDisplayNameIsNotEmpty()
    {
        var service = new SystemInfoService();
        var info = await service.GetComputerInfoAsync();

        Assert.False(string.IsNullOrWhiteSpace(info.EditionDisplayName));
    }

    [Fact]
    public async Task GetComputerInfoAsync_RespectsCancellation()
    {
        var service = new SystemInfoService();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.GetComputerInfoAsync(cts.Token));
    }
}