using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Models;
using Xunit;

namespace MqPrinterFixer.Tests;

public class FirewallStatusParserTests
{
    [Fact]
    public void Parse_NullInput_ReturnsUnknown()
    {
        Assert.Same(FirewallStatus.Unknown, FirewallStatusParser.Parse(null));
        Assert.Same(FirewallStatus.Unknown, FirewallStatusParser.Parse(""));
    }

    [Fact]
    public void Parse_MalformedJson_ReturnsUnknown()
    {
        Assert.Same(FirewallStatus.Unknown, FirewallStatusParser.Parse("not json"));
    }

    [Fact]
    public void Parse_ArrayOfTwoGroups_ReturnsBothChecks()
    {
        var json = """
        [
            {"GroupName":"File and Printer Sharing","TotalRules":10,"EnabledRules":10},
            {"GroupName":"Network Discovery","TotalRules":8,"EnabledRules":0}
        ]
        """;

        var status = FirewallStatusParser.Parse(json);

        Assert.Equal(2, status.Checks.Count);
        Assert.Equal(FirewallRuleStatus.Enabled,
            status.Checks.First(c => c.GroupName == "File and Printer Sharing").Status);
        Assert.Equal(FirewallRuleStatus.Disabled,
            status.Checks.First(c => c.GroupName == "Network Discovery").Status);
        Assert.Equal(FirewallRuleStatus.Partial, status.OverallStatus);
    }

    [Fact]
    public void Parse_SingleObject_Works()
    {
        var json = """{"GroupName":"File and Printer Sharing","TotalRules":10,"EnabledRules":10}""";

        var status = FirewallStatusParser.Parse(json);

        Assert.Single(status.Checks);
        Assert.Equal(FirewallRuleStatus.Enabled, status.OverallStatus);
    }

    [Fact]
    public void Parse_ZeroRules_NotApplicable()
    {
        var json = """{"GroupName":"Missing Group","TotalRules":0,"EnabledRules":0}""";

        var status = FirewallStatusParser.Parse(json);

        Assert.Single(status.Checks);
        Assert.Equal(FirewallRuleStatus.NotApplicable, status.Checks[0].Status);
        Assert.Equal(FirewallRuleStatus.NotApplicable, status.OverallStatus);
    }

    [Fact]
    public void Parse_FriendlyNameMapping()
    {
        var json = """{"GroupName":"File and Printer Sharing","TotalRules":10,"EnabledRules":10}""";
        var status = FirewallStatusParser.Parse(json);
        Assert.Equal("File & Printer Sharing", status.Checks[0].DisplayName);
    }

    [Fact]
    public void Summary_ReportsCounts()
    {
        var json = """
        [
            {"GroupName":"File and Printer Sharing","TotalRules":10,"EnabledRules":10},
            {"GroupName":"Network Discovery","TotalRules":8,"EnabledRules":0}
        ]
        """;

        var status = FirewallStatusParser.Parse(json);

        Assert.Contains("1/2", status.Summary);
    }
}