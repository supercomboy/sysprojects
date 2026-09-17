namespace MqPrinterFixer.App.Models;

/// <summary>
/// Trạng thái của một firewall rule group.
/// </summary>
public enum FirewallRuleStatus
{
    Unknown = 0,

    /// <summary>Tất cả rules trong group đều enabled.</summary>
    Enabled,

    /// <summary>Tất cả rules trong group đều disabled.</summary>
    Disabled,

    /// <summary>Một số rules enabled, một số disabled.</summary>
    Partial,

    /// <summary>Group không tồn tại trên Windows này (rule đã bị xóa hoặc không áp dụng).</summary>
    NotApplicable
}