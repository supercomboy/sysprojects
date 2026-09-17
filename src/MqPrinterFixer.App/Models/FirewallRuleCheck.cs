namespace MqPrinterFixer.App.Models;

/// <summary>
/// Kết quả kiểm tra một rule group cụ thể.
/// </summary>
public sealed record FirewallRuleCheck(
    string GroupName,
    FirewallRuleStatus Status,
    int TotalRules,
    int EnabledRules,
    string DisplayName);