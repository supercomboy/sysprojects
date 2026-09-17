using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Models;
using Xunit;

namespace MqPrinterFixer.Tests;

public class FirewallStatusAggregatorTests
{
    // ============================================================
    //   ComputeRuleStatus
    // ============================================================

    [Theory]
    [InlineData(0, 0, FirewallRuleStatus.NotApplicable)]
    [InlineData(0, 5, FirewallRuleStatus.NotApplicable)]
    [InlineData(10, 0, FirewallRuleStatus.Disabled)]
    [InlineData(10, 10, FirewallRuleStatus.Enabled)]
    [InlineData(10, 15, FirewallRuleStatus.Enabled)]
    [InlineData(10, 3, FirewallRuleStatus.Partial)]
    [InlineData(10, 9, FirewallRuleStatus.Partial)]
    public void ComputeRuleStatus_ReturnsExpected(
        int total, int enabled, FirewallRuleStatus expected)
    {
        Assert.Equal(expected, FirewallStatusAggregator.ComputeRuleStatus(total, enabled));
    }

    // ============================================================
    //   ComputeOverallStatus
    // ============================================================

    [Fact]
    public void ComputeOverallStatus_EmptyList_ReturnsUnknown()
    {
        Assert.Equal(FirewallRuleStatus.Unknown,
            FirewallStatusAggregator.ComputeOverallStatus(Array.Empty<FirewallRuleCheck>()));
    }

    [Fact]
    public void ComputeOverallStatus_AllEnabled_ReturnsEnabled()
    {
        var checks = new[]
        {
            Make("A", FirewallRuleStatus.Enabled),
            Make("B", FirewallRuleStatus.Enabled)
        };
        Assert.Equal(FirewallRuleStatus.Enabled,
            FirewallStatusAggregator.ComputeOverallStatus(checks));
    }

    [Fact]
    public void ComputeOverallStatus_AllDisabled_ReturnsDisabled()
    {
        var checks = new[]
        {
            Make("A", FirewallRuleStatus.Disabled),
            Make("B", FirewallRuleStatus.Disabled)
        };
        Assert.Equal(FirewallRuleStatus.Disabled,
            FirewallStatusAggregator.ComputeOverallStatus(checks));
    }

    [Fact]
    public void ComputeOverallStatus_Mixed_ReturnsPartial()
    {
        var checks = new[]
        {
            Make("A", FirewallRuleStatus.Enabled),
            Make("B", FirewallRuleStatus.Disabled)
        };
        Assert.Equal(FirewallRuleStatus.Partial,
            FirewallStatusAggregator.ComputeOverallStatus(checks));
    }

    [Fact]
    public void ComputeOverallStatus_AllNotApplicable_ReturnsNotApplicable()
    {
        var checks = new[]
        {
            Make("A", FirewallRuleStatus.NotApplicable),
            Make("B", FirewallRuleStatus.NotApplicable)
        };
        Assert.Equal(FirewallRuleStatus.NotApplicable,
            FirewallStatusAggregator.ComputeOverallStatus(checks));
    }

    [Fact]
    public void ComputeOverallStatus_IgnoresNotApplicable()
    {
        var checks = new[]
        {
            Make("A", FirewallRuleStatus.Enabled),
            Make("B", FirewallRuleStatus.NotApplicable)
        };
        Assert.Equal(FirewallRuleStatus.Enabled,
            FirewallStatusAggregator.ComputeOverallStatus(checks));
    }

    [Fact]
    public void ComputeOverallStatus_HasUnknown_ReturnsUnknown()
    {
        var checks = new[]
        {
            Make("A", FirewallRuleStatus.Enabled),
            Make("B", FirewallRuleStatus.Unknown)
        };
        Assert.Equal(FirewallRuleStatus.Unknown,
            FirewallStatusAggregator.ComputeOverallStatus(checks));
    }

    [Theory]
    [InlineData(FirewallRuleStatus.Enabled, "Enabled")]
    [InlineData(FirewallRuleStatus.Disabled, "Disabled")]
    [InlineData(FirewallRuleStatus.Partial, "Partial")]
    [InlineData(FirewallRuleStatus.NotApplicable, "Not Applicable")]
    [InlineData(FirewallRuleStatus.Unknown, "Unknown")]
    public void ToDisplayName(FirewallRuleStatus status, string expected)
    {
        Assert.Equal(expected, FirewallStatusAggregator.ToDisplayName(status));
    }

    private static FirewallRuleCheck Make(string name, FirewallRuleStatus status) =>
        new(GroupName: name, Status: status, TotalRules: 10, EnabledRules: 5, DisplayName: name);
}