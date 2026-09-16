using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Models;
using Xunit;

namespace MqPrinterFixer.Tests;

public class PrinterConnectionClassifierTests
{
    [Theory]
    [InlineData("USB001",            PrinterConnectionType.Usb)]
    [InlineData("USB002",            PrinterConnectionType.Usb)]
    [InlineData("usb003",            PrinterConnectionType.Usb)]
    public void Classify_UsbPorts(string port, PrinterConnectionType expected)
    {
        Assert.Equal(expected, PrinterConnectionClassifier.Classify(port, null));
    }

    [Theory]
    [InlineData(@"\\PC-KETOAN-01\Canon2900", PrinterConnectionType.NetworkShared)]
    [InlineData(@"\\SERVER\HP-LaserJet",     PrinterConnectionType.NetworkShared)]
    public void Classify_UncPaths(string port, PrinterConnectionType expected)
    {
        Assert.Equal(expected, PrinterConnectionClassifier.Classify(port, null));
    }

    [Theory]
    [InlineData("IP_192.168.1.100", PrinterConnectionType.TcpIp)]
    [InlineData("192.168.1.100",    PrinterConnectionType.TcpIp)]
    [InlineData("10.0.0.5",         PrinterConnectionType.TcpIp)]
    [InlineData("192.168.1.100:9100", PrinterConnectionType.TcpIp)]
    public void Classify_TcpIpPorts(string port, PrinterConnectionType expected)
    {
        Assert.Equal(expected, PrinterConnectionClassifier.Classify(port, null));
    }

    [Theory]
    [InlineData("WSD-abc12345-6789-...", PrinterConnectionType.Wsd)]
    [InlineData("wsd-1234",              PrinterConnectionType.Wsd)]
    public void Classify_WsdPorts(string port, PrinterConnectionType expected)
    {
        Assert.Equal(expected, PrinterConnectionClassifier.Classify(port, null));
    }

    [Theory]
    [InlineData("nul:",        null,                                   PrinterConnectionType.Virtual)]
    [InlineData("PORTPROMPT:", null,                                   PrinterConnectionType.Virtual)]
    [InlineData("SHRFAX:",     null,                                   PrinterConnectionType.Virtual)]
    [InlineData("USB001",      "Microsoft Print to PDF",               PrinterConnectionType.Virtual)]
    [InlineData("PORTPROMPT:", "Microsoft XPS Document Writer",        PrinterConnectionType.Virtual)]
    [InlineData("nul:",        "OneNote (Desktop)",                    PrinterConnectionType.Virtual)]
    public void Classify_VirtualPrinters(string port, string? printerName, PrinterConnectionType expected)
    {
        Assert.Equal(expected, PrinterConnectionClassifier.Classify(port, printerName));
    }

    [Theory]
    [InlineData(null,    null, PrinterConnectionType.Unknown)]
    [InlineData("",      null, PrinterConnectionType.Unknown)]
    [InlineData("HP_Custom_Port_1", null, PrinterConnectionType.Unknown)]
    public void Classify_UnknownCases(string? port, string? printerName, PrinterConnectionType expected)
    {
        Assert.Equal(expected, PrinterConnectionClassifier.Classify(port, printerName));
    }

    [Fact]
    public void Classify_InvalidIpLike_NotTcpIp()
    {
        // 4 octet nhưng có octet > 255
        Assert.NotEqual(PrinterConnectionType.TcpIp,
            PrinterConnectionClassifier.Classify("300.1.1.1", null));

        // Chỉ 3 octet
        Assert.NotEqual(PrinterConnectionType.TcpIp,
            PrinterConnectionClassifier.Classify("192.168.1", null));
    }
}