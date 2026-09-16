using Microsoft.Extensions.DependencyInjection;
using MqPrinterFixer.App;
using MqPrinterFixer.App.Interfaces;
using Xunit;

namespace MqPrinterFixer.Tests;

public class ComputerServiceTests
{
    [Fact]
    public async Task RenameComputerAsync_InvalidName_ReturnsFailure()
    {
        using var provider = BuildProvider();
        var svc = provider.GetRequiredService<IComputerService>();

        var result = await svc.RenameComputerAsync("invalid name with space");

        Assert.False(result.Success);
        Assert.False(result.RestartRequired);
        Assert.NotEmpty(result.ErrorMessage);
    }

    [Fact]
    public async Task RenameComputerAsync_SameAsCurrent_ReturnsFailure()
    {
        using var provider = BuildProvider();
        var svc = provider.GetRequiredService<IComputerService>();
        var sysInfo = provider.GetRequiredService<ISystemInfoService>();

        var info = await sysInfo.GetComputerInfoAsync();
        var result = await svc.RenameComputerAsync(info.ComputerName);

        Assert.False(result.Success);
        Assert.NotEmpty(result.ErrorMessage);
    }

    /// <summary>
    /// Test này KHÔNG thực hiện rename thật. Nếu môi trường test đang không elevated,
    /// service sẽ trả về lỗi "cần Admin" – đây là hành vi đúng cần verify.
    /// </summary>
    [Fact]
    public async Task RenameComputerAsync_WhenNotElevated_ReturnsAdminError()
    {
        using var provider = BuildProvider();
        var svc = provider.GetRequiredService<IComputerService>();
        var sysInfo = provider.GetRequiredService<ISystemInfoService>();

        var info = await sysInfo.GetComputerInfoAsync();

        // Chỉ chạy assert khi process đang không elevated (an toàn).
        if (!info.IsAdministrator)
        {
            var result = await svc.RenameComputerAsync("PC-NEW-TEST");
            Assert.False(result.Success);
            Assert.Contains("Administrator", result.ErrorMessage);
        }
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddMqPrinterFixerServices();
        return services.BuildServiceProvider();
    }
}