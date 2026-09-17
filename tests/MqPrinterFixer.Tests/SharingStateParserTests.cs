using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Models;
using Xunit;

namespace MqPrinterFixer.Tests;

public class SharingStateParserTests
{
    [Fact]
    public void Parse_NullInput_ReturnsUnknown()
    {
        Assert.Same(SharingState.Unknown, SharingStateParser.Parse(null));
        Assert.Same(SharingState.Unknown, SharingStateParser.Parse(""));
    }

    [Fact]
    public void Parse_MalformedJson_ReturnsUnknown()
    {
        Assert.Same(SharingState.Unknown, SharingStateParser.Parse("not json"));
    }

    [Fact]
    public void Parse_FullValidJson_ReturnsState()
    {
        var json = """
        {
            "NetworkCategory":"Private",
            "InterfaceAlias":"Ethernet",
            "NetworkDiscoveryEnabled":true,
            "FileAndPrinterSharingEnabled":false,
            "PasswordProtectedSharingEnabled":true
        }
        """;

        var state = SharingStateParser.Parse(json);

        Assert.Equal(NetworkCategory.Private, state.NetworkCategory);
        Assert.Equal("Ethernet", state.InterfaceAlias);
        Assert.True(state.NetworkDiscoveryEnabled);
        Assert.False(state.FileAndPrinterSharingEnabled);
        Assert.True(state.PasswordProtectedSharingEnabled);
    }

    [Fact]
    public void Parse_NullInterfaceAlias_UsesDash()
    {
        var json = """{"InterfaceAlias":null,"NetworkCategory":"Public"}""";
        var state = SharingStateParser.Parse(json);
        Assert.Equal("—", state.InterfaceAlias);
    }
}