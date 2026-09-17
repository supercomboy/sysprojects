using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Models;
using Xunit;

namespace MqPrinterFixer.Tests;

public class PrinterRoleMapperTests
{
    private const string Local = "PC-LOCAL";

    // ---------- Helpers tạo PrinterInfo gọn ----------
    private static PrinterInfo P(
        string name = "P",
        bool shared = false,
        string shareName = "",
        string sourceHost = "",
        PrinterConnectionType type = PrinterConnectionType.Usb)
        => new(
            Name: name,
            Status: PrinterStatus.Ready,
            IsDefault: false,
            ConnectionType: type,
            PortName: "USB001",
            DriverName: "D",
            IsShared: shared,
            ShareName: shareName,
            SourceHost: sourceHost,
            PrinterPath: string.Empty,
            Location: string.Empty,
            Comment: string.Empty);

    // ============================================================
    //   ComputeRole
    // ============================================================

    [Fact]
    public void ComputeRole_EmptyList_ReturnsStandalone()
    {
        var role = PrinterRoleMapper.ComputeRole(Array.Empty<PrinterInfo>(), Local);
        Assert.Equal(PrinterRole.Standalone, role);
    }

    [Fact]
    public void ComputeRole_OneLocalUsbNotShared_ReturnsStandalone()
    {
        var printers = new[] { P(shared: false) };
        Assert.Equal(PrinterRole.Standalone,
            PrinterRoleMapper.ComputeRole(printers, Local));
    }

    [Fact]
    public void ComputeRole_OneLocalUsbShared_ReturnsHost()
    {
        var printers = new[] { P(shared: true, shareName: "Canon2900") };
        Assert.Equal(PrinterRole.Host,
            PrinterRoleMapper.ComputeRole(printers, Local));
    }

    [Fact]
    public void ComputeRole_SharedTrueButNoShareName_ReturnsStandalone()
    {
        // Shared=true nhưng ShareName rỗng — Windows sẽ không share
        var printers = new[] { P(shared: true, shareName: "") };
        Assert.Equal(PrinterRole.Standalone,
            PrinterRoleMapper.ComputeRole(printers, Local));
    }

    [Fact]
    public void ComputeRole_UsbAlone_NotCountedAsHost()
    {
        // Master Prompt Section 9: USB printer không đủ để suy luận Host.
        var printers = new[]
        {
            P("Canon LBP2900", shared: false, shareName: "", type: PrinterConnectionType.Usb)
        };
        Assert.Equal(PrinterRole.Standalone,
            PrinterRoleMapper.ComputeRole(printers, Local));
    }

    [Fact]
    public void ComputeRole_OneRemoteShared_ReturnsClient()
    {
        var printers = new[]
        {
            P("Remote Printer",
              shared: false,
              shareName: "",
              sourceHost: "PC-KETOAN-01",
              type: PrinterConnectionType.NetworkShared)
        };
        Assert.Equal(PrinterRole.Client,
            PrinterRoleMapper.ComputeRole(printers, Local));
    }

    [Fact]
    public void ComputeRole_LocalSharedAndRemoteShared_ReturnsHybrid()
    {
        var printers = new[]
        {
            P("Local Shared", shared: true, shareName: "Canon2900"),
            P("Remote Printer",
              shared: false,
              shareName: "",
              sourceHost: "PC-KETOAN-01",
              type: PrinterConnectionType.NetworkShared)
        };
        Assert.Equal(PrinterRole.Hybrid,
            PrinterRoleMapper.ComputeRole(printers, Local));
    }

    [Fact]
    public void ComputeRole_SelfSharedPrinter_NotRemote()
    {
        // SourceHost = máy hiện tại → không tính là remote
        var printers = new[]
        {
            P("Local Shared", shared: true, shareName: "S", sourceHost: Local)
        };
        Assert.Equal(PrinterRole.Host,
            PrinterRoleMapper.ComputeRole(printers, Local));
    }

    [Fact]
    public void ComputeRole_CaseInsensitiveHostComparison()
    {
        var printers = new[]
        {
            P("P", shared: false, sourceHost: "pc-local")   // lowercase
        };
        // So với Local = "PC-LOCAL" — không phải remote
        Assert.Equal(PrinterRole.Standalone,
            PrinterRoleMapper.ComputeRole(printers, Local));
    }

    [Fact]
    public void ComputeRole_EmptyLocalName_RemoteStillDetected()
    {
        var printers = new[]
        {
            P("P", sourceHost: "PC-KETOAN-01",
              type: PrinterConnectionType.NetworkShared)
        };
        // local name = "" → mọi SourceHost non-empty đều là remote
        Assert.Equal(PrinterRole.Client,
            PrinterRoleMapper.ComputeRole(printers, ""));
    }

    // ============================================================
    //   ApplyOverride
    // ============================================================

    [Theory]
    [InlineData(PrinterRole.Standalone)]
    [InlineData(PrinterRole.Host)]
    [InlineData(PrinterRole.Client)]
    [InlineData(PrinterRole.Hybrid)]
    public void ApplyOverride_Auto_ReturnsDetected(PrinterRole detected)
    {
        Assert.Equal(detected,
            PrinterRoleMapper.ApplyOverride(detected, RoleDetectionMode.Auto));
    }

    [Theory]
    [InlineData(PrinterRole.Standalone)]
    [InlineData(PrinterRole.Host)]
    [InlineData(PrinterRole.Client)]
    [InlineData(PrinterRole.Hybrid)]
    public void ApplyOverride_ForceHost_AlwaysReturnsHost(PrinterRole detected)
    {
        Assert.Equal(PrinterRole.Host,
            PrinterRoleMapper.ApplyOverride(detected, RoleDetectionMode.ForceHost));
    }

    [Theory]
    [InlineData(PrinterRole.Standalone)]
    [InlineData(PrinterRole.Host)]
    [InlineData(PrinterRole.Client)]
    [InlineData(PrinterRole.Hybrid)]
    public void ApplyOverride_ForceClient_AlwaysReturnsClient(PrinterRole detected)
    {
        Assert.Equal(PrinterRole.Client,
            PrinterRoleMapper.ApplyOverride(detected, RoleDetectionMode.ForceClient));
    }

    [Theory]
    [InlineData(PrinterRole.Standalone)]
    [InlineData(PrinterRole.Host)]
    [InlineData(PrinterRole.Client)]
    [InlineData(PrinterRole.Hybrid)]
    public void ApplyOverride_ForceHybrid_AlwaysReturnsHybrid(PrinterRole detected)
    {
        Assert.Equal(PrinterRole.Hybrid,
            PrinterRoleMapper.ApplyOverride(detected, RoleDetectionMode.ForceHybrid));
    }

    // ============================================================
    //   Display names
    // ============================================================

    [Theory]
    [InlineData(PrinterRole.Standalone, "Standalone")]
    [InlineData(PrinterRole.Host,       "Printer Host")]
    [InlineData(PrinterRole.Client,     "Printer Client")]
    [InlineData(PrinterRole.Hybrid,     "Host + Client")]
    public void ToDisplayName_PrinterRole(PrinterRole role, string expected)
    {
        Assert.Equal(expected, PrinterRoleMapper.ToDisplayName(role));
    }

    [Theory]
    [InlineData(RoleDetectionMode.Auto,        "Auto Detect")]
    [InlineData(RoleDetectionMode.ForceHost,   "Printer Host")]
    [InlineData(RoleDetectionMode.ForceClient, "Printer Client")]
    [InlineData(RoleDetectionMode.ForceHybrid, "Host + Client")]
    public void ToDisplayName_RoleDetectionMode(RoleDetectionMode mode, string expected)
    {
        Assert.Equal(expected, PrinterRoleMapper.ToDisplayName(mode));
    }
}