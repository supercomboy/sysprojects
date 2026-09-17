using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Interfaces;

public interface IFirewallService
{
    /// <summary>
    /// Kiểm tra trạng thái các rule group cần thiết cho printer sharing.
    /// Read-only.
    /// </summary>
    Task<FirewallStatus> CheckAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Bật tất cả rules trong các group cần thiết (chỉ Private profile).
    /// KHÔNG tắt firewall. Verify sau khi apply.
    /// </summary>
    Task<NetworkChangeResult> EnableRequiredRulesAsync(
        CancellationToken cancellationToken = default);
}