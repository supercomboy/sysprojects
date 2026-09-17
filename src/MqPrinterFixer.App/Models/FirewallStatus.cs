namespace MqPrinterFixer.App.Models;

/// <summary>
/// Tổng hợp trạng thái firewall cho printer/network sharing.
/// </summary>
public sealed record FirewallStatus(
    FirewallRuleStatus OverallStatus,
    IReadOnlyList<FirewallRuleCheck> Checks)
{
    public static readonly FirewallStatus Unknown = new(
        FirewallRuleStatus.Unknown,
        Array.Empty<FirewallRuleCheck>());

    public bool HasAnyEnabled =>
        Checks.Any(c => c.Status == FirewallRuleStatus.Enabled);

    public string Summary
    {
        get
        {
            if (Checks.Count == 0)
            {
                return "Không có dữ liệu firewall.";
            }

            var enabled = Checks.Count(c => c.Status == FirewallRuleStatus.Enabled);
            return $"{enabled}/{Checks.Count} rule groups enabled";
        }
    }
}