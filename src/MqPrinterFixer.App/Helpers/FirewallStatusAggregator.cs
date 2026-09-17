using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Helpers;

/// <summary>
/// Tính status của 1 group từ số rules enabled / total.
/// Pure logic, testable 100%.
/// </summary>
public static class FirewallStatusAggregator
{
    public static FirewallRuleStatus ComputeRuleStatus(int totalRules, int enabledRules)
    {
        if (totalRules <= 0)
        {
            return FirewallRuleStatus.NotApplicable;
        }

        if (enabledRules <= 0)
        {
            return FirewallRuleStatus.Disabled;
        }

        if (enabledRules >= totalRules)
        {
            return FirewallRuleStatus.Enabled;
        }

        return FirewallRuleStatus.Partial;
    }

    /// <summary>
    /// Tổng hợp trạng thái của nhiều rule group thành overall status.
    /// Logic:
    ///   - Không có check nào → Unknown
    ///   - Tất cả NotApplicable → NotApplicable
    ///   - Tất cả Enabled → Enabled
    ///   - Tất cả Disabled → Disabled
    ///   - Mix → Partial
    /// </summary>
    public static FirewallRuleStatus ComputeOverallStatus(
        IReadOnlyList<FirewallRuleCheck> checks)
    {
        if (checks.Count == 0)
        {
            return FirewallRuleStatus.Unknown;
        }

        var statuses = checks.Select(c => c.Status).ToArray();

        // Nếu tất cả NotApplicable → NotApplicable
        if (statuses.All(s => s == FirewallRuleStatus.NotApplicable))
        {
            return FirewallRuleStatus.NotApplicable;
        }

        // Bỏ qua NotApplicable khi tính
        var relevant = statuses
            .Where(s => s != FirewallRuleStatus.NotApplicable)
            .ToArray();

        if (relevant.Length == 0)
        {
            return FirewallRuleStatus.NotApplicable;
        }

        if (relevant.All(s => s == FirewallRuleStatus.Enabled))
        {
            return FirewallRuleStatus.Enabled;
        }

        if (relevant.All(s => s == FirewallRuleStatus.Disabled))
        {
            return FirewallRuleStatus.Disabled;
        }

        // Có ít nhất 1 Unknown → Unknown (không đủ tin cậy)
        if (relevant.Any(s => s == FirewallRuleStatus.Unknown))
        {
            return FirewallRuleStatus.Unknown;
        }

        return FirewallRuleStatus.Partial;
    }

    public static string ToDisplayName(FirewallRuleStatus status) => status switch
    {
        FirewallRuleStatus.Enabled       => "Enabled",
        FirewallRuleStatus.Disabled      => "Disabled",
        FirewallRuleStatus.Partial       => "Partial",
        FirewallRuleStatus.NotApplicable => "Not Applicable",
        _                                => "Unknown"
    };
}