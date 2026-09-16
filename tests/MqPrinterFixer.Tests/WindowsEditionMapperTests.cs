using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Models;
using Xunit;

namespace MqPrinterFixer.Tests;

public class WindowsEditionMapperTests
{
    [Theory]
    [InlineData("Professional",    WindowsEdition.Pro)]
    [InlineData("ProfessionalN",   WindowsEdition.Pro)]
    [InlineData("Core",            WindowsEdition.Home)]
    [InlineData("CoreSingleLanguage", WindowsEdition.Home)]
    [InlineData("Enterprise",      WindowsEdition.Enterprise)]
    [InlineData("Education",       WindowsEdition.Education)]
    [InlineData("ProfessionalEducation", WindowsEdition.ProEducation)]
    [InlineData("ProfessionalWorkstation", WindowsEdition.ProWorkstation)]
    [InlineData("ServerStandard",  WindowsEdition.ServerStandard)]
    [InlineData("ServerDatacenter", WindowsEdition.ServerDatacenter)]
    [InlineData("SomethingWeird",  WindowsEdition.Unknown)]
    public void Map_EditionId_ReturnsExpected(string editionId, WindowsEdition expected)
    {
        var result = WindowsEditionMapper.Map(editionId, null);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Windows 10 Pro",           WindowsEdition.Pro)]
    [InlineData("Windows 11 Home",          WindowsEdition.Home)]
    [InlineData("Windows 10 Enterprise",    WindowsEdition.Enterprise)]
    public void Map_FallbackToProductName_WhenEditionIdNull(string productName, WindowsEdition expected)
    {
        var result = WindowsEditionMapper.Map(null, productName);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Map_NullBoth_ReturnsUnknown()
    {
        Assert.Equal(WindowsEdition.Unknown, WindowsEditionMapper.Map(null, null));
        Assert.Equal(WindowsEdition.Unknown, WindowsEditionMapper.Map("", ""));
    }

    [Fact]
    public void BuildDisplayName_Windows11Fix_WhenBuildGreaterThan22000()
    {
        var result = WindowsEditionMapper.BuildDisplayName(
            "Windows 10 Pro", WindowsEdition.Pro, "23H2", 22631);

        Assert.Equal("Windows 11 Pro 23H2", result);
    }

    [Fact]
    public void BuildDisplayName_Windows10_KeptWhenBuildBelow()
    {
        var result = WindowsEditionMapper.BuildDisplayName(
            "Windows 10 Pro", WindowsEdition.Pro, "22H2", 19045);

        Assert.Equal("Windows 10 Pro 22H2", result);
    }

    [Fact]
    public void BuildDisplayName_NoProductName_UsesEditionEnum()
    {
        var result = WindowsEditionMapper.BuildDisplayName(
            null, WindowsEdition.Pro, null, 19045);

        Assert.Equal("Windows (Pro)", result);
    }
}