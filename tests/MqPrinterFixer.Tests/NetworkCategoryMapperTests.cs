using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Models;
using Xunit;

namespace MqPrinterFixer.Tests;

public class NetworkCategoryMapperTests
{
    [Theory]
    [InlineData("Public", NetworkCategory.Public)]
    [InlineData("public", NetworkCategory.Public)]
    [InlineData("PUBLIC", NetworkCategory.Public)]
    [InlineData("Private", NetworkCategory.Private)]
    [InlineData("private", NetworkCategory.Private)]
    [InlineData("DomainAuthenticated", NetworkCategory.DomainAuthenticated)]
    [InlineData("domainauthenticated", NetworkCategory.DomainAuthenticated)]
    [InlineData("Domain", NetworkCategory.DomainAuthenticated)]
    public void Parse_ValidStrings(string input, NetworkCategory expected)
    {
        Assert.Equal(expected, NetworkCategoryMapper.Parse(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Garbage")]
    public void Parse_InvalidStrings_ReturnsUnknown(string? input)
    {
        Assert.Equal(NetworkCategory.Unknown, NetworkCategoryMapper.Parse(input));
    }

    [Theory]
    [InlineData(NetworkCategory.Public, "Public")]
    [InlineData(NetworkCategory.Private, "Private")]
    [InlineData(NetworkCategory.DomainAuthenticated, "DomainAuthenticated")]
    [InlineData(NetworkCategory.Unknown, "Unknown")]
    public void ToDisplayName(NetworkCategory category, string expected)
    {
        Assert.Equal(expected, NetworkCategoryMapper.ToDisplayName(category));
    }

    [Theory]
    [InlineData(NetworkCategory.Public, true)]
    [InlineData(NetworkCategory.Private, false)]
    [InlineData(NetworkCategory.DomainAuthenticated, false)]
    [InlineData(NetworkCategory.Unknown, false)]
    public void IsPublic(NetworkCategory category, bool expected)
    {
        Assert.Equal(expected, NetworkCategoryMapper.IsPublic(category));
    }

    [Theory]
    [InlineData(NetworkCategory.DomainAuthenticated, true)]
    [InlineData(NetworkCategory.Public, false)]
    [InlineData(NetworkCategory.Private, false)]
    [InlineData(NetworkCategory.Unknown, false)]
    public void IsProtectedFromChange(NetworkCategory category, bool expected)
    {
        Assert.Equal(expected, NetworkCategoryMapper.IsProtectedFromChange(category));
    }
}