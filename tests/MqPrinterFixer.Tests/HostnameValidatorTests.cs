using MqPrinterFixer.App.Helpers;
using Xunit;

namespace MqPrinterFixer.Tests;

public class HostnameValidatorTests
{
    private const string Current = "PC-OLD";

    [Theory]
    [InlineData("PC-NEW")]
    [InlineData("SERVER01")]
    [InlineData("a")]
    [InlineData("PC-KETOAN-01")]
    [InlineData("123abc")]
    public void Validate_ValidNames_ReturnValid(string name)
    {
        var result = HostnameValidator.Validate(name, Current);
        Assert.True(result.IsValid);
        Assert.Equal(name, result.NormalizedName);
        Assert.Empty(result.ErrorMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_NullOrEmpty_ReturnsInvalid(string? name)
    {
        var result = HostnameValidator.Validate(name, Current);
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.ErrorMessage);
    }

    [Fact]
    public void Validate_TooLong_ReturnsInvalid()
    {
        var name = new string('a', 16);
        var result = HostnameValidator.Validate(name, Current);
        Assert.False(result.IsValid);
        Assert.Contains("15", result.ErrorMessage);
    }

    [Fact]
    public void Validate_Exactly15Chars_IsValid()
    {
        var name = new string('a', 15);
        var result = HostnameValidator.Validate(name, Current);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("PC NEW")]
    [InlineData("PC_NEW")]
    [InlineData("PC.NEW")]
    [InlineData("PC!NEW")]
    [InlineData("PC@NEW")]
    public void Validate_InvalidCharacters_ReturnInvalid(string name)
    {
        var result = HostnameValidator.Validate(name, Current);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("-PCNEW")]
    [InlineData("PCNEW-")]
    public void Validate_LeadingOrTrailingHyphen_ReturnInvalid(string name)
    {
        var result = HostnameValidator.Validate(name, Current);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678")]
    public void Validate_AllDigits_ReturnsInvalid(string name)
    {
        var result = HostnameValidator.Validate(name, Current);
        Assert.False(result.IsValid);
        Assert.Contains("số", result.ErrorMessage);
    }

    [Fact]
    public void Validate_SameAsCurrent_CaseInsensitive_ReturnsInvalid()
    {
        var result = HostnameValidator.Validate("pc-old", Current);
        Assert.False(result.IsValid);
        Assert.Contains("giống", result.ErrorMessage);
    }

    [Fact]
    public void Validate_TrimsWhitespace()
    {
        var result = HostnameValidator.Validate("  PC-NEW  ", Current);
        Assert.True(result.IsValid);
        Assert.Equal("PC-NEW", result.NormalizedName);
    }
}