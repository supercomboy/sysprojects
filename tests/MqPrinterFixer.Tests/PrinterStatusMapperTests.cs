using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Models;
using Xunit;

namespace MqPrinterFixer.Tests;

public class PrinterStatusMapperTests
{
    [Fact]
    public void Map_WorkOffline_AlwaysReturnsOffline()
    {
        // Kể cả PrinterStatus = Idle
        var status = PrinterStatusMapper.Map(
            workOffline: true,
            printerStatus: 3,
            detectedErrorState: 2);

        Assert.Equal(PrinterStatus.Offline, status);
    }

    [Theory]
    [InlineData(3, PrinterStatus.Ready)]  // Idle
    [InlineData(4, PrinterStatus.Ready)]  // Printing
    [InlineData(5, PrinterStatus.Ready)]  // Warmup
    public void Map_ReadyStates(int wmiPrinterStatus, PrinterStatus expected)
    {
        var status = PrinterStatusMapper.Map(
            workOffline: false,
            printerStatus: wmiPrinterStatus,
            detectedErrorState: 2);
        Assert.Equal(expected, status);
    }

    [Fact]
    public void Map_StoppedPrinting_ReturnsPaused()
    {
        var status = PrinterStatusMapper.Map(false, 6, 2);
        Assert.Equal(PrinterStatus.Paused, status);
    }

    [Fact]
    public void Map_Offline_ReturnsOffline()
    {
        var status = PrinterStatusMapper.Map(false, 7, 2);
        Assert.Equal(PrinterStatus.Offline, status);
    }

    [Theory]
    [InlineData(3, 4, PrinterStatus.Error)]   // Idle nhưng No Paper
    [InlineData(3, 9, PrinterStatus.Error)]   // Idle nhưng Offline state
    [InlineData(3, 10, PrinterStatus.Error)]  // Service Requested
    public void Map_DetectedError_ReturnsError(
        int printerStatus, int detectedErrorState, PrinterStatus expected)
    {
        var status = PrinterStatusMapper.Map(false, printerStatus, detectedErrorState);
        Assert.Equal(expected, status);
    }

    [Fact]
    public void Map_UnknownState_ReturnsUnknown()
    {
        var status = PrinterStatusMapper.Map(false, 2, 2);
        Assert.Equal(PrinterStatus.Unknown, status);
    }

    [Fact]
    public void Map_NullPrinterStatus_ReturnsUnknown()
    {
        var status = PrinterStatusMapper.Map(false, null, 2);
        Assert.Equal(PrinterStatus.Unknown, status);
    }

    [Theory]
    [InlineData(PrinterStatus.Ready,   "Ready")]
    [InlineData(PrinterStatus.Offline, "Offline")]
    [InlineData(PrinterStatus.Paused,  "Paused")]
    [InlineData(PrinterStatus.Error,   "Error")]
    [InlineData(PrinterStatus.Unknown, "Unknown")]
    public void ToDisplayName_PrinterStatus(PrinterStatus status, string expected)
    {
        Assert.Equal(expected, PrinterStatusMapper.ToDisplayName(status));
    }

    [Theory]
    [InlineData(PrinterConnectionType.Usb,           "USB")]
    [InlineData(PrinterConnectionType.TcpIp,         "TCP/IP")]
    [InlineData(PrinterConnectionType.Wsd,           "WSD")]
    [InlineData(PrinterConnectionType.NetworkShared, "Network Shared")]
    [InlineData(PrinterConnectionType.Virtual,       "Virtual")]
    [InlineData(PrinterConnectionType.Unknown,       "Unknown")]
    public void ToDisplayName_ConnectionType(PrinterConnectionType type, string expected)
    {
        Assert.Equal(expected, PrinterStatusMapper.ToDisplayName(type));
    }
}