using MqPrinterFixer.App.Models;
using MqPrinterFixer.App.Services;
using MqPrinterFixer.Tests.Fakes;
using Xunit;

namespace MqPrinterFixer.Tests;

public class NetworkSharingServiceTests
{
    [Fact]
    public async Task GetSharingStateAsync_PowerShellFails_ReturnsUnknown()
    {
        var fake = new FakePowerShellService
        {
            NextResult = PowerShellResult.Fail(1, "error")
        };
        var svc = new NetworkSharingService(fake, new SystemInfoService());

        var state = await svc.GetSharingStateAsync();

        Assert.Same(SharingState.Unknown, state);
    }

    [Fact]
    public async Task ApplyChangeAsync_WhenPreviewCannotApply_ReturnsBlocked()
    {
        var fake = new FakePowerShellService();
        var svc = new NetworkSharingService(fake, new SystemInfoService());

        var preview = new NetworkChangePreview(
            Action: NetworkChangeAction.ChangeNetworkToPrivate,
            Title: "Test",
            Reason: "Test",
            CurrentState: "Private",
            ProposedState: "Private",
            CanApply: false,
            BlockReason: "Already Private",
            RequiresAdmin: false,
            RequiresConfirmation: false);

        var result = await svc.ApplyChangeAsync(preview);

        Assert.False(result.Success);
        Assert.Equal("Already Private", result.ErrorMessage);
        Assert.Equal(0, fake.CallCount);
    }
}