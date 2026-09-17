using MqPrinterFixer.App.Models;
using MqPrinterFixer.App.Services;
using MqPrinterFixer.Tests.Fakes;
using Xunit;

namespace MqPrinterFixer.Tests;

public class NetworkServiceTests
{
    [Fact]
    public async Task GetActiveNetworkAsync_PowerShellFails_ReturnsEmpty()
    {
        var fake = new FakePowerShellService
        {
            NextResult = PowerShellResult.Fail(1, "some error")
        };
        var svc = new NetworkService(fake);

        var info = await svc.GetActiveNetworkAsync();

        Assert.False(info.IsConnected);
        Assert.Equal(NetworkCategory.Unknown, info.Category);
    }

    [Fact]
    public async Task GetActiveNetworkAsync_EmptyJsonArray_ReturnsEmpty()
    {
        var fake = new FakePowerShellService
        {
            NextResult = PowerShellResult.Ok("[]")
        };
        var svc = new NetworkService(fake);

        var info = await svc.GetActiveNetworkAsync();

        Assert.False(info.IsConnected);
    }

    [Fact]
    public async Task GetActiveNetworkAsync_SingleProfile_Parsed()
    {
        var json = """
        {
            "InterfaceAlias": "Ethernet",
            "Name": "HomeNet",
            "NetworkCategory": "Private",
            "IPv4Address": "192.168.1.10",
            "MacAddress": "AA-BB-CC-DD-EE-FF"
        }
        """;

        var fake = new FakePowerShellService { NextResult = PowerShellResult.Ok(json) };
        var svc = new NetworkService(fake);

        var info = await svc.GetActiveNetworkAsync();

        Assert.True(info.IsConnected);
        Assert.Equal("Ethernet", info.AdapterName);
        Assert.Equal("HomeNet", info.NetworkName);
        Assert.Equal(NetworkCategory.Private, info.Category);
        Assert.Equal("192.168.1.10", info.IPv4Address);
        Assert.Equal("AA-BB-CC-DD-EE-FF", info.MacAddress);
    }

    [Fact]
    public async Task GetActiveNetworkAsync_MultipleProfiles_PrefersNonDomainWithIp()
    {
        var json = """
        [
            {
                "InterfaceAlias": "VPN",
                "Name": "CorpVPN",
                "NetworkCategory": "DomainAuthenticated",
                "IPv4Address": "10.0.0.5",
                "MacAddress": ""
            },
            {
                "InterfaceAlias": "Wi-Fi",
                "Name": "HomeNet",
                "NetworkCategory": "Private",
                "IPv4Address": "192.168.1.10",
                "MacAddress": "AA-BB"
            }
        ]
        """;

        var fake = new FakePowerShellService { NextResult = PowerShellResult.Ok(json) };
        var svc = new NetworkService(fake);

        var info = await svc.GetActiveNetworkAsync();

        Assert.Equal("Wi-Fi", info.AdapterName);
        Assert.Equal(NetworkCategory.Private, info.Category);
    }

    [Fact]
    public async Task GetActiveNetworkAsync_MalformedJson_ReturnsEmpty()
    {
        var fake = new FakePowerShellService
        {
            NextResult = PowerShellResult.Ok("not a json {")
        };
        var svc = new NetworkService(fake);

        var info = await svc.GetActiveNetworkAsync();

        Assert.False(info.IsConnected);
    }

    [Fact]
    public async Task GetActiveNetworkAsync_NullFields_UsesPlaceholders()
    {
        var json = """
        {
            "InterfaceAlias": null,
            "Name": null,
            "NetworkCategory": "Public",
            "IPv4Address": null,
            "MacAddress": null
        }
        """;

        var fake = new FakePowerShellService { NextResult = PowerShellResult.Ok(json) };
        var svc = new NetworkService(fake);

        var info = await svc.GetActiveNetworkAsync();

        Assert.True(info.IsConnected);
        Assert.Equal("—", info.AdapterName);
        Assert.Equal("—", info.IPv4Address);
        Assert.Equal(NetworkCategory.Public, info.Category);
    }
}