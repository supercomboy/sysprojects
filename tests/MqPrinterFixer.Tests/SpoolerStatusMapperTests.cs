using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Models;
using Xunit;

namespace MqPrinterFixer.Tests;

public class SpoolerStatusMapperTests
{
    [Theory]
    [InlineData("Running",        SpoolerStatus.Running)]
    [InlineData("Stopped",        SpoolerStatus.Stopped)]
    [InlineData("StartPending",   SpoolerStatus.Starting)]
    [InlineData("StopPending",    SpoolerStatus.Stopping)]
    [InlineData("Paused",         SpoolerStatus.Paused)]
    [InlineData("PausePending",   SpoolerStatus.Paused)]
    [InlineData("ContinuePending",SpoolerStatus.Starting)]
    public void FromServiceControllerStatus_KnownValues(string input, SpoolerStatus expected)
    {
        Assert.Equal(expected, SpoolerStatusMapper.FromServiceControllerStatus(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("WeirdStatus")]
    public void FromServiceControllerStatus_UnknownValues_ReturnsUnknown(string? input)
    {
        Assert.Equal(SpoolerStatus.Unknown,
            SpoolerStatusMapper.FromServiceControllerStatus(input));
    }

    [Theory]
    [InlineData(SpoolerStatus.Running,  "Running")]
    [InlineData(SpoolerStatus.Stopped,  "Stopped")]
    [InlineData(SpoolerStatus.Starting, "Starting")]
    [InlineData(SpoolerStatus.Stopping, "Stopping")]
    [InlineData(SpoolerStatus.Paused,   "Paused")]
    [InlineData(SpoolerStatus.Unknown,  "Unknown")]
    public void ToDisplayName(SpoolerStatus status, string expected)
    {
        Assert.Equal(expected, SpoolerStatusMapper.ToDisplayName(status));
    }

    [Theory]
    [InlineData(SpoolerStatus.Stopped, true)]
    [InlineData(SpoolerStatus.Running, false)]
    [InlineData(SpoolerStatus.Paused,  false)]
    [InlineData(SpoolerStatus.Unknown, false)]
    public void CanStart_OnlyWhenStopped(SpoolerStatus status, bool expected)
    {
        Assert.Equal(expected, SpoolerStatusMapper.CanStart(status));
    }

    [Theory]
    [InlineData(SpoolerStatus.Running, true)]
    [InlineData(SpoolerStatus.Paused,  true)]
    [InlineData(SpoolerStatus.Stopped, false)]
    [InlineData(SpoolerStatus.Unknown, false)]
    public void CanRestart_WhenRunningOrPaused(SpoolerStatus status, bool expected)
    {
        Assert.Equal(expected, SpoolerStatusMapper.CanRestart(status));
    }
}